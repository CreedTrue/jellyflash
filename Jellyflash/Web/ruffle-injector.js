// Jellyflash - Ruffle Injector for Jellyfin

(function() {
    console.log("Jellyflash: Initializing Ruffle Injector");

    // Load Ruffle from CDN
    let script = document.createElement("script");
    script.src = "https://unpkg.com/@ruffle-rs/ruffle";
    document.head.appendChild(script);

    let ruffle = null;
    let rufflePlayer = null;

    script.onload = () => {
        window.RufflePlayer = window.RufflePlayer || {};
        ruffle = window.RufflePlayer.newest();
        console.log("Jellyflash: Ruffle loaded");
    };

    function startRuffle(streamUrl, title) {
        if (!ruffle) return;

        // Create overlay
        let overlay = document.createElement("div");
        overlay.id = "jellyflash-overlay";
        overlay.style.position = "fixed";
        overlay.style.top = "0";
        overlay.style.left = "0";
        overlay.style.width = "100%";
        overlay.style.height = "100%";
        overlay.style.backgroundColor = "black";
        overlay.style.zIndex = "99999";
        overlay.style.display = "flex";
        overlay.style.flexDirection = "column";

        // Create Header
        let header = document.createElement("div");
        header.style.padding = "10px";
        header.style.backgroundColor = "rgba(0,0,0,0.8)";
        header.style.color = "white";
        header.style.display = "flex";
        header.style.justifyContent = "space-between";
        
        let titleEl = document.createElement("span");
        titleEl.innerText = "Jellyflash: " + title;
        header.appendChild(titleEl);

        let closeBtn = document.createElement("button");
        closeBtn.innerText = "Close";
        closeBtn.style.cursor = "pointer";
        closeBtn.onclick = () => {
            if (rufflePlayer) {
                rufflePlayer.remove();
                rufflePlayer = null;
            }
            document.body.removeChild(overlay);
        };
        header.appendChild(closeBtn);
        overlay.appendChild(header);

        // Create Player Container
        let container = document.createElement("div");
        container.style.flex = "1";
        overlay.appendChild(container);

        document.body.appendChild(overlay);

        // Mount Ruffle
        rufflePlayer = ruffle.createPlayer();
        container.appendChild(rufflePlayer);
        rufflePlayer.style.width = "100%";
        rufflePlayer.style.height = "100%";
        
        rufflePlayer.load(streamUrl);
    }

    // Observe DOM for the detail page
    const observer = new MutationObserver((mutations) => {
        let playButtons = document.querySelectorAll('.btnPlay');
        if (playButtons.length > 0 && window.ApiClient) {
            // Check current URL for item ID
            let urlParams = new URLSearchParams(window.location.hash.split('?')[1]);
            let itemId = urlParams.get('id');

            if (itemId) {
                // Ensure we only inject once per item view
                if (!document.getElementById("jellyflash-btn-" + itemId)) {
                    // Fetch item details
                    window.ApiClient.getItem(window.ApiClient.getCurrentUserId(), itemId).then(item => {
                        if (item && item.Path && item.Path.toLowerCase().endsWith(".swf")) {
                            console.log("Jellyflash: Detected SWF item, injecting button");
                            
                            let targetContainer = playButtons[0].parentNode;
                            let btn = document.createElement("button");
                            btn.id = "jellyflash-btn-" + itemId;
                            btn.className = "raised btnPlay emby-button";
                            btn.innerHTML = '<span class="material-icons play_arrow"></span><span class="emby-button-text">Play with Ruffle</span>';
                            
                            btn.onclick = (e) => {
                                e.preventDefault();
                                e.stopPropagation();
                                
                                // The download URL gives the raw file, which is perfect for Ruffle
                                let streamUrl = window.ApiClient.getUrl("Items/" + itemId + "/Download", {
                                    api_key: window.ApiClient.accessToken()
                                });
                                
                                startRuffle(streamUrl, item.Name);
                            };

                            targetContainer.insertBefore(btn, playButtons[0].nextSibling);
                        }
                    });
                }
            }
        }
    });

    observer.observe(document.body, { childList: true, subtree: true });
})();
