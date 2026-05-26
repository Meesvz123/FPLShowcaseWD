(async () => {
    const slots = Array.from(document.querySelectorAll(".slot"));
    const panel = document.getElementById("player-panel");
    const panelTitle = document.getElementById("panel-title");
    const panelClose = document.getElementById("panel-close");
    const optionsList = document.getElementById("player-options");
    const saveButton = document.getElementById("save-team");
    const status = document.getElementById("save-status");
    const csrfToken = document.querySelector('meta[name="csrf-token"]')?.getAttribute("content") ?? "";
    const teamId = document.body.dataset.teamId;
    const teamName = document.body.dataset.teamName;
    const teamQuery = teamId ? `?teamId=${encodeURIComponent(teamId)}` : "";
    const teamUrl = `/api/fantasy/team${teamQuery}`;

    const setStatus = (message, isError = false) => {
        if (!status) return;
        status.textContent = message;
        status.dataset.state = isError ? "error" : "ok";
    };

    const formatPrice = (value) => {
        if (!value || Number.isNaN(value)) return "€-";
        const millions = value / 1_000_000;
        return `€${millions.toFixed(1)}m`;
    };

    const response = await fetch("/api/fantasy/players");
    if (!response.ok) {
        setStatus("Spelers laden mislukt.", true);
        return;
    }
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

        slot.replaceChildren();

        const nameSpan = document.createElement("span");
        nameSpan.className = "slot-name";
        nameSpan.textContent = player.naam;

        const removeButton = document.createElement("button");
        removeButton.type = "button";
        removeButton.className = "slot-remove";
        removeButton.setAttribute("aria-label", "Verwijder speler");
        removeButton.textContent = "×";

        slot.append(nameSpan, removeButton);
    };

    const loadTeam = async () => {
        const teamResponse = await fetch(teamUrl);
        if (teamResponse.status === 404) return;

        if (!teamResponse.ok) {
            setStatus("Team laden mislukt.", true);
            return;
        }

        const team = await teamResponse.json();
        slots.forEach(clearSlot);

        team.slots.forEach(slotInfo => {
            const slot = slots[slotInfo.slotIndex];
            if (!slot) return;

            const player = players.find(p => Number(p.id) === slotInfo.playerId);
            if (!player) return;

            setSlotPlayer(slot, player);
        });
    };

    await loadTeam();

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
            btn.textContent = `${player.naam} (${player.club}) • ${player.positie} • ${formatPrice(player.prijs)}`;
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

    document.addEventListener("keydown", (e) => {
        if (e.key === "Escape" && !panel.hidden) {
            panel.hidden = true;
            activeSlot = null;
            clearSelection();
        }
    });

    panel.addEventListener("click", (e) => {
        if (e.target === panel) {
            panel.hidden = true;
            activeSlot = null;
        }
    });

    const buildTeamPayload = () => {
        const slotsPayload = [];
        slots.forEach((slot, index) => {
            const playerId = Number(slot.dataset.playerId);
            if (!playerId) return;

            slotsPayload.push({
                playerId,
                position: slot.dataset.position,
                area: slot.dataset.area,
                slotIndex: index
            });
        });

        return {
            name: teamName?.trim() || "Mijn team",
            slots: slotsPayload
        };
    };

    saveButton.addEventListener("click", async () => {
        const payload = buildTeamPayload();

        if (payload.slots.length === 0) {
            setStatus("Kies eerst spelers.", true);
            return;
        }

        if (!csrfToken) {
            setStatus("CSRF-token ontbreekt.", true);
            return;
        }

        const result = await fetch(teamUrl, {
            method: "POST",
            headers: {
                "Content-Type": "application/json",
                "X-CSRF-TOKEN": csrfToken
            },
            body: JSON.stringify(payload)
        });

        if (!result.ok) {
            const text = await result.text();
            setStatus(`Opslaan mislukt: ${text}`, true);
            return;
        }

        setStatus("Team opgeslagen.");
    });
})();