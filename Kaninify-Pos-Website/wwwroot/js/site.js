window.productAddedAlert = function () {
    alert('Product added to the table!');
};

window.scrollTableToBottom = function () {
    // Target the actual scrollable container instead of the table
    var container = document.querySelector('.checkout-table-container');
    if (!container) {
        return;
    }

    var doScroll = function () {
        container.scrollTop = container.scrollHeight;
    };

    // Wait for layout to settle after Blazor renders
    requestAnimationFrame(function () {
        setTimeout(doScroll, 0);
    });
};

window.focusElement = function (element) {
    element.focus();
};

// Print content function
window.printContent = function (content) {
    const printWindow = window.open('', '_blank');
    printWindow.document.write(content);
    printWindow.document.close();
    printWindow.focus();
    printWindow.print();
    printWindow.close();
};

// Download file function
window.downloadFile = function (filename, content, contentType) {
    const blob = new Blob([content], { type: contentType });
    const url = window.URL.createObjectURL(blob);
    const a = document.createElement('a');
    a.href = url;
    a.download = filename;
    document.body.appendChild(a);
    a.click();
    document.body.removeChild(a);
    window.URL.revokeObjectURL(url);
};

// QR Code generation function using QRCode.js library
window.generateQRCode = (elementId, text) => {
    const element = document.getElementById(elementId);
    if (element && window.QRCode) {
        // Clear any existing content
        element.innerHTML = '';
        
        // Generate actual QR code that will work for navigation
        QRCode.toCanvas(element, text, {
            width: 120,
            height: 120,
            margin: 2,
            color: {
                dark: '#000000',
                light: '#ffffff'
            },
            errorCorrectionLevel: 'M'
        }, function (error) {
            if (error) {
                console.error('QR Code generation failed:', error);
                // Fallback: show text link
                element.innerHTML = `<a href="${text}" target="_blank" style="font-size: 12px; color: #666;">${text}</a>`;
            }
        });
    } else {
        // Fallback if QRCode library is not available
        console.warn('QRCode library not available');
        element.innerHTML = `<a href="${text}" target="_blank" style="font-size: 12px; color: #666;">${text}</a>`;
    }
};

async function ActivateCameraToReadBarcode() {
    console.log("ActivateCameraToReadBarcode");

    try {
        // ✅ Check if camera permissions are already granted
        const permission = await navigator.permissions.query({ name: "camera" });
        if (permission.state === "denied") {
            alert("Camera permission denied! Please allow access in browser settings.");
            console.error("Camera permission denied by user.");
            return;
        }

        // ✅ Request camera access with advanced settings
        const stream = await navigator.mediaDevices.getUserMedia({
            video: {
                focusMode: "continuous", // Keep autofocus enabled
                frameRate: { ideal: 60 }, // Higher FPS for better scanning
                facingMode: "environment" // Prefer rear camera
            }
        });

        console.log("Camera access granted");
        startScanner();
    } catch (err) {
        alert("Camera access denied! Ensure your browser allows camera access.");
        console.error("Camera error:", err);
    }
}

async function startScanner() {
    console.log("startScanner");

    let scanner;
    if (!scanner) {
        scanner = new Html5Qrcode("reader");
        console.log("new Html5Qrcode");
    }

    document.getElementById("reader").style.display = "block";

    try {
        scanner.start(
            { facingMode: "environment" }, // Use rear camera
            {
                fps: 60, // High FPS for smoother scanning
                qrbox: { width: 300, height: 300 }, // Larger QR box for better detection
                disableFlip: true // Prevent mirroring
            },
            (decodedText) => {
                document.getElementById("inputBarcode").value = decodedText;
                document.getElementById("triggerSearchProduct").click();
                scanner.stop();
                document.getElementById("reader").style.display = "none";
            },
            (errorMessage) => {
                console.log("Scanner error:", errorMessage);
            }
        );
    } catch (err) {
        console.error("Scanner initialization error:", err);
        alert("Failed to start barcode scanner.");
    }
}

async function GetBarcodeValue() {
    console.log("GetBarcodeValue");
    var barcodeValue = document.getElementById("inputBarcode").value;
    console.log("barcodeValue: " + barcodeValue);
    return barcodeValue;
}


 
