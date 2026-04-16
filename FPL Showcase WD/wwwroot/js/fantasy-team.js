(async () => {
    const slots = document.querySelectorAll(".slot");
    const panel = document.getElementById("player-panel");
    const panelTitle = document.getElementById("panel-title");
    const panelClose = document.getElementById("panel-close");
    const optionsList = document.getElementById("player-options");

    const response = await fetch("/api/fantasy/players");
    const players = await response.json();

    let activeSlot = null;
    let selectedSlot = null;

    const clearSlot = (slot) => {
        slot.dataset.playerId = "";
        slot.dataset.playerName = "";
        slot.classList.remove("slot--filled");
        slot.textContent = slot.dataset.placeholder ?? "+";
    };

    const setSlotPlayer = (slot, player) => {
        slot.dataset.playerId = player.id;
        slot.dataset.playerName = player.naam;
        slot.classList.add("slot--filled");
        slot.innerHTML = `
            <span class="slot-name">${player.naam}</span>
            <span class="slot-remove" title="Verwijder speler" aria-hidden="true">×</span>
        `;
    };

    const getSelectedIds = () => {
        const ids = new Set();
        slots.forEach(slot => {
            if (slot.dataset.playerId) {
                ids.add(slot.dataset.playerId);
            }
        });
        return ids;
    };

    const clearSelection = () => {
        if (selectedSlot) {
            selectedSlot.classList.remove("slot--selected");
            selectedSlot = null;
        }
    };

    const openPanel = (slot) => {
        activeSlot = slot;
        const position = slot.dataset.position;
        const selectedIds = getSelectedIds();
        const currentId = slot.dataset.playerId;

        const filtered = players
            .filter(p => position === "BENCH" || p.positie === position)
            .filter(p => !selectedIds.has(String(p.id)) || String(p.id) === currentId);

        panelTitle.textContent = `Kies ${position}`;
        optionsList.innerHTML = "";

        filtered.forEach(player => {
            const li = document.createElement("li");
            const btn = document.createElement("button");
            btn.type = "button";
            btn.className = "panel-option";
            btn.textContent = `${player.naam} (${player.club})`;
            btn.addEventListener("click", () => {
                setSlotPlayer(activeSlot, player);
                panel.hidden = true;
                activeSlot = null;
            });
            li.appendChild(btn);
            optionsList.appendChild(li);
        });

        panel.hidden = false;
    };

    slots.forEach(slot => {
        slot.addEventListener("click", (e) => {
            if (e.target.closest(".slot-remove")) {
                clearSlot(slot);
                clearSelection();
                return;
            }

            const isFilled = !!slot.dataset.playerName;

            if (!isFilled) {
                clearSelection();
                openPanel(slot);
                return;
            }

            if (!selectedSlot) {
                selectedSlot = slot;
                selectedSlot.classList.add("slot--selected");
                return;
            }

            if (selectedSlot === slot) {
                clearSelection();
                return;
            }

            const sameRole = selectedSlot.dataset.position === slot.dataset.position;
            if (sameRole) {
                const a = {
                    id: selectedSlot.dataset.playerId,
                    naam: selectedSlot.dataset.playerName
                };
                const b = {
                    id: slot.dataset.playerId,
                    naam: slot.dataset.playerName
                };
                setSlotPlayer(selectedSlot, b);
                setSlotPlayer(slot, a);
            }

            clearSelection();
        });
    });

    panelClose.addEventListener("click", () => {
        panel.hidden = true;
        activeSlot = null;
    });

    panel.addEventListener("click", (e) => {
        if (e.target === panel) {
            panel.hidden = true;
            activeSlot = null;
        }
    });
})();