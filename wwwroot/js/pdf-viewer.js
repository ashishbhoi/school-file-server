// PDF Viewer with page navigation
class PDFViewer {
    constructor(containerId, pdfUrl) {
        this.container = document.getElementById(containerId);
        this.pdfUrl = pdfUrl;
        this.pdfDoc = null;
        this.currentPage = 1;
        this.totalPages = 0;
        this.scale = 1.5;
        this.canvas = null;
        this.ctx = null;

        this.init();
    }

    async init() {
        // Set up PDF.js worker
        pdfjsLib.GlobalWorkerOptions.workerSrc = "/lib/pdfjs/pdf.worker.min.js";

        try {
            // Load the PDF document
            this.pdfDoc = await pdfjsLib.getDocument(this.pdfUrl).promise;
            this.totalPages = this.pdfDoc.numPages;

            // Create the UI
            this.createUI();

            // Render the first page
            await this.renderPage(1);
        } catch (error) {
            console.error("Error loading PDF:", error);
            this.showError("Failed to load PDF document");
        }
    }

    createUI() {
        this.container.innerHTML = `
            <div class="pdf-controls bg-gray-800 text-white p-4 flex items-center justify-between rounded-t-lg">
                <div class="flex items-center space-x-4">
                    <button id="prev-page" class="bg-blue-600 hover:bg-blue-700 px-4 py-2 rounded-lg font-medium touch-target touch-manipulation disabled:opacity-50 disabled:cursor-not-allowed">
                        ← Previous
                    </button>
                    <span id="page-info" class="font-medium">Page 1 of ${
            this.totalPages
        }</span>
                    <button id="next-page" class="bg-blue-600 hover:bg-blue-700 px-4 py-2 rounded-lg font-medium touch-target touch-manipulation disabled:opacity-50 disabled:cursor-not-allowed">
                        Next →
                    </button>
                </div>
                <div class="flex items-center space-x-4">
                    <button id="zoom-out" class="bg-gray-600 hover:bg-gray-700 px-3 py-2 rounded-lg touch-target touch-manipulation">
                        −
                    </button>
                    <span id="zoom-level">${Math.round(
            this.scale * 100
        )}%</span>
                    <button id="zoom-in" class="bg-gray-600 hover:bg-gray-700 px-3 py-2 rounded-lg touch-target touch-manipulation">
                        +
                    </button>
                    <button id="fullscreen-btn" class="bg-green-600 hover:bg-green-700 px-4 py-2 rounded-lg font-medium touch-target touch-manipulation">
                        ⛶ Fullscreen
                    </button>
                </div>
            </div>
            <div class="pdf-canvas-container bg-gray-100 p-4 text-center overflow-auto" style="max-height: 70vh;">
                <canvas id="pdf-canvas" class="mx-auto shadow-lg"></canvas>
            </div>
            <div id="loading" class="text-center py-8 hidden">
                <div class="inline-block animate-spin rounded-full h-8 w-8 border-b-2 border-blue-600"></div>
                <p class="mt-2">Loading page...</p>
            </div>
        `;

        // Set up canvas
        this.canvas = document.getElementById("pdf-canvas");
        this.ctx = this.canvas.getContext("2d");

        // Set up event listeners
        this.setupEventListeners();
    }

    setupEventListeners() {
        // Previous page button
        document.getElementById("prev-page").addEventListener("click", () => {
            if (this.currentPage > 1) {
                this.goToPage(this.currentPage - 1);
            }
        });

        // Next page button
        document.getElementById("next-page").addEventListener("click", () => {
            if (this.currentPage < this.totalPages) {
                this.goToPage(this.currentPage + 1);
            }
        });

        // Zoom controls
        document.getElementById("zoom-in").addEventListener("click", () => {
            this.scale = Math.min(this.scale * 1.2, 6.0);
            this.renderPage(this.currentPage);
            this.updateZoomDisplay();
        });

        document.getElementById("zoom-out").addEventListener("click", () => {
            this.scale = Math.max(this.scale / 1.2, 0.5);
            this.renderPage(this.currentPage);
            this.updateZoomDisplay();
        });

        // Fullscreen button
        document.getElementById("fullscreen-btn").addEventListener("click", () => {
            this.toggleFullscreen();
        });

        // Keyboard navigation
        document.addEventListener("keydown", (e) => {
            if (e.key === "ArrowLeft" && this.currentPage > 1) {
                this.goToPage(this.currentPage - 1);
            } else if (e.key === "ArrowRight" && this.currentPage < this.totalPages) {
                this.goToPage(this.currentPage + 1);
            }
        });

        // Touch gestures for mobile
        let startX = 0;
        this.canvas.addEventListener("touchstart", (e) => {
            startX = e.touches[0].clientX;
        });

        this.canvas.addEventListener("touchend", (e) => {
            const endX = e.changedTouches[0].clientX;
            const diff = startX - endX;

            if (Math.abs(diff) > 50) {
                // Minimum swipe distance
                if (diff > 0 && this.currentPage < this.totalPages) {
                    // Swipe left - next page
                    this.goToPage(this.currentPage + 1);
                } else if (diff < 0 && this.currentPage > 1) {
                    // Swipe right - previous page
                    this.goToPage(this.currentPage - 1);
                }
            }
        });
    }

    async goToPage(pageNum) {
        if (pageNum >= 1 && pageNum <= this.totalPages) {
            this.currentPage = pageNum;
            await this.renderPage(pageNum);
            this.updateControls();
        }
    }

    async renderPage(pageNum) {
        this.showLoading(true);

        try {
            // Get the page
            const page = await this.pdfDoc.getPage(pageNum);

            // Calculate viewport
            const viewport = page.getViewport({scale: this.scale});

            // Set canvas dimensions
            this.canvas.width = viewport.width;
            this.canvas.height = viewport.height;

            // Render the page
            const renderContext = {
                canvasContext: this.ctx,
                viewport: viewport,
            };

            await page.render(renderContext).promise;
        } catch (error) {
            console.error("Error rendering page:", error);
            this.showError(`Failed to render page ${pageNum}`);
        } finally {
            this.showLoading(false);
        }
    }

    updateControls() {
        // Update page info
        document.getElementById(
            "page-info"
        ).textContent = `Page ${this.currentPage} of ${this.totalPages}`;

        // Update button states
        document.getElementById("prev-page").disabled = this.currentPage === 1;
        document.getElementById("next-page").disabled =
            this.currentPage === this.totalPages;
    }

    updateZoomDisplay() {
        document.getElementById("zoom-level").textContent = `${Math.round(
            this.scale * 100
        )}%`;
    }

    showLoading(show) {
        const loading = document.getElementById("loading");
        if (show) {
            loading.classList.remove("hidden");
        } else {
            loading.classList.add("hidden");
        }
    }

    showError(message) {
        this.container.innerHTML = `
            <div class="text-center py-8">
                <div class="text-red-600 text-xl mb-4">⚠️ Error</div>
                <p class="text-gray-600">${message}</p>
            </div>
        `;
    }

    toggleFullscreen() {
        if (!document.fullscreenElement) {
            // Enter fullscreen
            this.container
                .requestFullscreen()
                .then(() => {
                    // Apply fullscreen styles
                    this.container.style.position = "fixed";
                    this.container.style.top = "0";
                    this.container.style.left = "0";
                    this.container.style.width = "100vw";
                    this.container.style.height = "100vh";
                    this.container.style.zIndex = "9999";
                    this.container.style.backgroundColor = "#000";

                    // Update canvas container for fullscreen
                    const canvasContainer = this.container.querySelector(
                        ".pdf-canvas-container"
                    );
                    canvasContainer.style.maxHeight = "calc(100vh - 80px)";
                    canvasContainer.style.height = "calc(100vh - 80px)";
                    canvasContainer.style.backgroundColor = "#000";
                    canvasContainer.style.display = "flex";
                    canvasContainer.style.alignItems = "center";
                    canvasContainer.style.justifyContent = "center";

                    // Scale canvas to fit screen while maintaining aspect ratio
                    this.fitCanvasToScreen();

                    // Update fullscreen button text
                    const fullscreenBtn = document.getElementById("fullscreen-btn");
                    fullscreenBtn.innerHTML = "⛶ Exit Fullscreen";
                })
                .catch((err) => {
                    console.error("Error attempting to enable fullscreen:", err);
                });
        } else {
            // Exit fullscreen
            document.exitFullscreen();
        }
    }

    fitCanvasToScreen() {
        if (document.fullscreenElement) {
            const canvasContainer = this.container.querySelector(
                ".pdf-canvas-container"
            );
            const availableHeight = window.innerHeight - 80; // Account for controls
            const availableWidth = window.innerWidth - 32; // Account for padding

            // Calculate scale to fit the page within available space
            const currentPage = this.currentPage;
            this.pdfDoc.getPage(currentPage).then((page) => {
                const viewport = page.getViewport({scale: 1.0});
                const scaleX = availableWidth / viewport.width;
                const scaleY = availableHeight / viewport.height;
                const optimalScale = Math.min(scaleX, scaleY) * 0.9; // 90% of available space

                this.scale = optimalScale;
                this.renderPage(currentPage);
                this.updateZoomDisplay();
            });
        }
    }
}

// Listen for fullscreen changes
document.addEventListener("fullscreenchange", () => {
    // Reset styles when exiting fullscreen
    if (!document.fullscreenElement) {
        const containers = document.querySelectorAll(".pdf-canvas-container");
        containers.forEach((container) => {
            const parentContainer = container.closest('[id$="container"]');
            if (parentContainer) {
                parentContainer.style.position = "";
                parentContainer.style.top = "";
                parentContainer.style.left = "";
                parentContainer.style.width = "";
                parentContainer.style.height = "";
                parentContainer.style.zIndex = "";
                parentContainer.style.backgroundColor = "";

                container.style.maxHeight = "70vh";
                container.style.height = "";
                container.style.backgroundColor = "";
                container.style.display = "";
                container.style.alignItems = "";
                container.style.justifyContent = "";

                // Update fullscreen button text
                const fullscreenBtn = document.getElementById("fullscreen-btn");
                if (fullscreenBtn) {
                    fullscreenBtn.innerHTML = "⛶ Fullscreen";
                }
            }
        });
    }
});

// Global function to initialize PDF viewer
window.initPDFViewer = function (containerId, pdfUrl) {
    return new PDFViewer(containerId, pdfUrl);
};
