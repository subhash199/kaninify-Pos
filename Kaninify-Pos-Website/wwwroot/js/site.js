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

        const overlay = document.createElement('div');
        overlay.style.position = 'fixed';
        overlay.style.inset = '0';
        overlay.style.background = 'rgba(0, 0, 0, 0.8)';
        overlay.style.display = 'flex';
        overlay.style.flexDirection = 'column';
        overlay.style.alignItems = 'center';
        overlay.style.justifyContent = 'center';
        overlay.style.zIndex = '9999';

        const video = document.createElement('video');
        video.style.width = '90%';
        video.style.maxWidth = '520px';
        video.style.borderRadius = '12px';
        video.setAttribute('autoplay', 'true');
        video.setAttribute('playsinline', 'true');

        const controls = document.createElement('div');
        controls.style.display = 'flex';
        controls.style.flexDirection = 'column';
        controls.style.alignItems = 'center';
        controls.style.gap = '8px';
        controls.style.marginTop = '12px';

        const manualInput = document.createElement('input');
        manualInput.type = 'text';
        manualInput.placeholder = 'Enter barcode';
        manualInput.style.padding = '8px 12px';
        manualInput.style.borderRadius = '8px';
        manualInput.style.border = '1px solid #ccc';
        manualInput.style.width = '220px';

        const useButton = document.createElement('button');
        useButton.textContent = 'Use';
        useButton.style.padding = '8px 16px';
        useButton.style.borderRadius = '8px';
        useButton.style.border = 'none';

        const closeButton = document.createElement('button');
        closeButton.textContent = 'Close';
        closeButton.style.padding = '8px 16px';
        closeButton.style.borderRadius = '8px';
        closeButton.style.border = 'none';

        overlay.appendChild(video);
        controls.appendChild(manualInput);
        controls.appendChild(useButton);
        controls.appendChild(closeButton);
        overlay.appendChild(controls);
        document.body.appendChild(overlay);

        let stream;
        let active = true;

        const cleanup = (value) => {
            if (!active) {
                return;
            }
            active = false;
            if (stream) {
                stream.getTracks().forEach((track) => track.stop());
            }
            overlay.remove();
            resolve(value ?? null);
        };

        closeButton.onclick = () => cleanup(null);
        useButton.onclick = () => {
            const value = (manualInput.value || '').trim();
            if (value) {
                cleanup(value);
            }
        };

        try {
            stream = await navigator.mediaDevices.getUserMedia({
                video: { facingMode: 'environment' },
                audio: false
            });
            video.srcObject = stream;
            await video.play();
        } catch {
            cleanup(null);
            return;
        }

        let detector = null;
        if (window.BarcodeDetector) {
            try {
                detector = new BarcodeDetector({
                    formats: ['ean_13', 'ean_8', 'upc_a', 'upc_e', 'code_128', 'code_39', 'qr_code']
                });
            } catch {
                detector = null;
            }
        }

        const scan = async () => {
            if (!active) {
                return;
            }
            try {
                if (detector) {
                    const barcodes = await detector.detect(video);
                    if (barcodes && barcodes.length > 0 && barcodes[0].rawValue) {
                        cleanup(barcodes[0].rawValue);
                        return;
                    }
                }
            } catch {
            }
            requestAnimationFrame(scan);
        };

        scan();
    });
};
