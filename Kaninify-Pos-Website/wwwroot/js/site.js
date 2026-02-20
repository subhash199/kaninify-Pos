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

window.startBarcodeScan = function () {
    return new Promise(async (resolve) => {
        if (!navigator.mediaDevices || !navigator.mediaDevices.getUserMedia) {
            resolve(null);
            return;
        }
        const ensureHtml5Qrcode = async () => {
            if (window.Html5Qrcode) {
                return true;
            }
            const loadScript = (src) => new Promise((resolveLoad, rejectLoad) => {
                const existing = Array.from(document.scripts).find((s) => s.src === src);
                if (existing) {
                    if (window.Html5Qrcode) {
                        resolveLoad(true);
                        return;
                    }
                    existing.addEventListener('load', () => resolveLoad(true));
                    existing.addEventListener('error', () => rejectLoad());
                    return;
                }
                const script = document.createElement('script');
                script.src = src;
                script.async = true;
                script.onload = () => resolveLoad(true);
                script.onerror = () => rejectLoad();
                document.head.appendChild(script);
            });
            try {
                await loadScript("https://cdn.jsdelivr.net/npm/html5-qrcode@2.3.10/minified/html5-qrcode.min.js");
                if (window.Html5Qrcode) {
                    return true;
                }
            } catch {
            }
            try {
                await loadScript("https://unpkg.com/html5-qrcode@2.3.10/minified/html5-qrcode.min.js");
                if (window.Html5Qrcode) {
                    return true;
                }
            } catch {
            }
            return false;
        };

        const hasScanner = await ensureHtml5Qrcode();
        if (!hasScanner) {
            alert("Barcode scanner not available.");
            resolve(null);
            return;
        }

        let overlay = document.getElementById('barcode-overlay');
        let reader = document.getElementById('barcode-reader');

        if (!overlay) {
            overlay = document.createElement('div');
            overlay.id = 'barcode-overlay';
            overlay.style.position = 'fixed';
            overlay.style.inset = '0';
            overlay.style.background = 'rgba(0, 0, 0, 0.8)';
            overlay.style.display = 'flex';
            overlay.style.flexDirection = 'column';
            overlay.style.alignItems = 'center';
            overlay.style.justifyContent = 'center';
            overlay.style.zIndex = '9999';

            reader = document.createElement('div');
            reader.id = 'barcode-reader';
            reader.style.width = '320px';
            reader.style.height = '320px';
            reader.style.background = '#000';
            reader.style.borderRadius = '12px';

            const closeButton = document.createElement('button');
            closeButton.textContent = 'Close';
            closeButton.style.marginTop = '12px';
            closeButton.style.padding = '8px 16px';
            closeButton.style.borderRadius = '8px';
            closeButton.style.border = 'none';
            closeButton.onclick = async () => {
                await cleanup(null);
            };

            overlay.appendChild(reader);
            overlay.appendChild(closeButton);
            document.body.appendChild(overlay);
        } else {
            overlay.style.display = 'flex';
        }

        let scanner = null;

        const cleanup = async (value) => {
            if (scanner) {
                try {
                    await scanner.stop();
                } catch {
                }
                try {
                    await scanner.clear();
                } catch {
                }
                scanner = null;
            }
            if (overlay) {
                overlay.style.display = 'none';
            }
            resolve(value ?? null);
        };

        const ActivateCameraToReadBarcode = async () => {
            console.log("ActivateCameraToReadBarcode");
            try {
                const permission = await navigator.permissions.query({ name: "camera" });
                if (permission.state === "denied") {
                    alert("Camera permission denied! Please allow access in browser settings.");
                    console.error("Camera permission denied by user.");
                    return;
                }
                const stream = await navigator.mediaDevices.getUserMedia({
                    video: {
                        focusMode: "continuous",
                        frameRate: { ideal: 60 },
                        facingMode: "environment"
                    }
                });
                stream.getTracks().forEach((track) => track.stop());
                console.log("Camera access granted");
                await startScanner();
            } catch (err) {
                alert("Camera access denied! Ensure your browser allows camera access.");
                console.error("Camera error:", err);
                await cleanup(null);
            }
        };

        const startScanner = async () => {
            console.log("startScanner");
            if (!scanner) {
                scanner = new Html5Qrcode("barcode-reader");
                console.log("new Html5Qrcode");
            }
            try {
                await scanner.start(
                    { facingMode: "environment" },
                    {
                        fps: 60,
                        qrbox: { width: 300, height: 300 },
                        disableFlip: true
                    },
                    (decodedText) => {
                        const input = document.getElementById("inputBarcode");
                        if (input) {
                            input.value = decodedText;
                            input.dispatchEvent(new Event("input", { bubbles: true }));
                        }
                        cleanup(decodedText);
                    },
                    (errorMessage) => {
                        console.log("Scanner error:", errorMessage);
                    }
                );
            } catch (err) {
                console.error("Scanner initialization error:", err);
                alert("Failed to start barcode scanner.");
                await cleanup(null);
            }
        };

        await ActivateCameraToReadBarcode();
    });
};



 
