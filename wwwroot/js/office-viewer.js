// Office Document Viewer JavaScript
// Handles Word (mammoth.js) and Excel (SheetJS) document viewing

function loadWordDocument(fileId, fileName) {
    const viewer = document.getElementById("word-viewer");
    viewer.innerHTML = `
        <div class="text-center py-8">
            <div class="inline-block animate-spin rounded-full h-8 w-8 border-b-2 border-blue-600"></div>
            <p class="mt-2 text-gray-600">Loading Word document...</p>
        </div>
    `;

    fetch(`/File/ViewInline/${fileId}`)
        .then((response) => response.arrayBuffer())
        .then((arrayBuffer) => {
            return mammoth.convertToHtml({arrayBuffer: arrayBuffer});
        })
        .then((result) => {
            const content = `
                <div class="prose prose-lg max-w-none p-6">
                    <div class="bg-blue-50 border border-blue-200 rounded-lg p-3 mb-6">
                        <h4 class="text-blue-900 font-semibold mb-1">📘 ${fileName}</h4>
                        <p class="text-blue-700 text-sm">Converted from Word document • Some formatting may differ from original</p>
                    </div>
                    <div class="word-content bg-white border rounded-lg p-6 shadow-sm">
                        ${result.value}
                    </div>
                </div>
            `;
            viewer.innerHTML = content;

            if (result.messages.length > 0) {
                console.log("Conversion messages:", result.messages);
            }
        })
        .catch((error) => {
            console.error("Error loading Word document:", error);
            showOfficeError();
        });
}

function loadExcelDocument(fileId, fileName) {
    const viewer = document.getElementById("excel-viewer");
    viewer.innerHTML = `
        <div class="text-center py-8">
            <div class="inline-block animate-spin rounded-full h-8 w-8 border-b-2 border-green-600"></div>
            <p class="mt-2 text-gray-600">Loading Excel spreadsheet...</p>
        </div>
    `;

    fetch(`/File/ViewInline/${fileId}`)
        .then((response) => response.arrayBuffer())
        .then((data) => {
            const workbook = XLSX.read(data, {type: "array"});

            let html = `
                <div class="bg-green-50 border border-green-200 rounded-lg p-3 mb-4">
                    <h4 class="text-green-900 font-semibold mb-1">📗 ${fileName}</h4>
                    <p class="text-green-700 text-sm">Excel Spreadsheet • ${workbook.SheetNames.length} sheet(s)</p>
                </div>
            `;

            // Create tabs for multiple sheets
            if (workbook.SheetNames.length > 1) {
                html += `<div class="flex flex-wrap gap-2 mb-4 border-b">`;
                workbook.SheetNames.forEach((sheetName, index) => {
                    const activeClass =
                        index === 0
                            ? "border-green-500 text-green-700 bg-green-50"
                            : "border-transparent text-gray-500 hover:text-gray-700 hover:bg-gray-50";
                    html += `
                        <button onclick="showSheet(${index})" 
                                class="sheet-tab px-4 py-2 text-sm font-medium rounded-t-lg border-b-2 transition-colors touch-target touch-manipulation ${activeClass}"
                                data-sheet="${index}">
                            ${sheetName}
                        </button>
                    `;
                });
                html += `</div>`;
            }

            // Generate content for each sheet
            workbook.SheetNames.forEach((sheetName, index) => {
                const worksheet = workbook.Sheets[sheetName];
                const htmlTable = XLSX.utils.sheet_to_html(worksheet);
                const hiddenClass = index === 0 ? "" : "hidden";

                html += `
                    <div class="sheet-content ${hiddenClass}" data-sheet="${index}">
                        <div class="overflow-auto bg-white border rounded-lg shadow-sm" style="max-height: 60vh;">
                            ${htmlTable}
                        </div>
                    </div>
                `;
            });

            viewer.innerHTML = html;

            // Apply custom styling to the generated table
            applyExcelTableStyling();
        })
        .catch((error) => {
            console.error("Error loading Excel document:", error);
            showOfficeError();
        });
}

function applyExcelTableStyling() {
    // Apply Tailwind classes to the generated Excel table
    const tables = document.querySelectorAll("#excel-viewer table");
    tables.forEach((table) => {
        table.className = "w-full border-collapse border border-gray-300 text-sm";

        // Style headers
        const headers = table.querySelectorAll("th");
        headers.forEach((th) => {
            th.className =
                "border border-gray-300 bg-gray-100 px-2 py-1 font-semibold text-left";
        });

        // Style cells
        const cells = table.querySelectorAll("td");
        cells.forEach((td) => {
            td.className = "border border-gray-300 px-2 py-1";
        });
    });
}

function showSheet(sheetIndex) {
    // Hide all sheets
    document.querySelectorAll(".sheet-content").forEach((content) => {
        content.classList.add("hidden");
    });

    // Show selected sheet
    const selectedSheet = document.querySelector(
        `[data-sheet="${sheetIndex}"].sheet-content`
    );
    if (selectedSheet) {
        selectedSheet.classList.remove("hidden");
    }

    // Update tab styles
    document.querySelectorAll(".sheet-tab").forEach((tab) => {
        tab.classList.remove("border-green-500", "text-green-700", "bg-green-50");
        tab.classList.add("border-transparent", "text-gray-500");
    });

    const selectedTab = document.querySelector(
        `[data-sheet="${sheetIndex}"].sheet-tab`
    );
    if (selectedTab) {
        selectedTab.classList.remove("border-transparent", "text-gray-500");
        selectedTab.classList.add(
            "border-green-500",
            "text-green-700",
            "bg-green-50"
        );
    }
}

function showOfficeError() {
    const errorDiv = document.getElementById("office-error");
    if (errorDiv) {
        errorDiv.classList.remove("hidden");
    }

    // Hide other viewers
    const wordViewer = document.getElementById("word-viewer");
    const excelViewer = document.getElementById("excel-viewer");
    const pptInfo = document.getElementById("powerpoint-info");

    if (wordViewer) wordViewer.style.display = "none";
    if (excelViewer) excelViewer.style.display = "none";
    if (pptInfo) pptInfo.style.display = "none";
}

function setupOfficeFullscreen() {
    const fullscreenBtn = document.getElementById("office-fullscreen");
    if (fullscreenBtn) {
        fullscreenBtn.addEventListener("click", function () {
            const container = document.getElementById("viewer-container");
            if (!document.fullscreenElement) {
                container.requestFullscreen().catch((err) => {
                    console.error("Error attempting to enable fullscreen:", err);
                });
            } else {
                document.exitFullscreen();
            }
        });
    }
}

function setupOfficeRefresh(fileId, fileName, fileType) {
    const refreshBtn = document.getElementById("refresh-viewer");
    if (refreshBtn) {
        refreshBtn.addEventListener("click", function () {
            if (fileType === ".docx" || fileType === ".doc") {
                loadWordDocument(fileId, fileName);
            } else if (fileType === ".xlsx" || fileType === ".xls") {
                loadExcelDocument(fileId, fileName);
            }
        });
    }
}
