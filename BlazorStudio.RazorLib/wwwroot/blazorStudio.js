﻿window.blazorStudio = {
    addToLocalStorage(key, value) {
         localStorage[key] = value;
    },
    readLocalStorage(key) {
         return localStorage[key];
    },
    getViewportDimensions: function() {
        // Use the specified window or the current window if no argument
        let w = window;

        // This works for all browsers except IE8 and before
        if (w.innerWidth != null) return { widthInPixels: w.innerWidth, heightInPixels: w.innerHeight };

        // For IE (or any browser) in Standards mode
        var d = w.document;
        if (document.compatMode == "CSS1Compat")
            return {
                widthInPixels: d.documentElement.clientWidth,
                heightInPixels: d.documentElement.clientHeight
            };

        // For browsers in Quirks mode
        return { widthInPixels: d.body.clientWidth, heightInPixels: d.body.clientHeight };
    },
    intersectionObserver: 0,
    dotNetObjectReferenceByVirtualizeCoordinateSystemElementId: new Map(),
    scrollIntoViewIfOutOfViewport: function (elementId) {
        const value = this.dotNetObjectReferenceByVirtualizeCoordinateSystemElementId.get(elementId);

        if (value.intersectionRatio >= 1) {
            return;
        }

        let element = document.getElementById(elementId);
        let plainTextEditorDisplay = document.getElementById(this.getPlainTextEditorId(value.plainTextEditorGuid));

        plainTextEditorDisplay.scrollTop = element.offsetTop - 5;
    },
    initializeIntersectionObserver: function () {
        let options = {
            rootMargin: '0px',
            threshold: [
                0.1
            ]
        }

        this.intersectionObserver = new IntersectionObserver((entries) =>
            this.handleThresholdChange(entries,
                this.dotNetObjectReferenceByVirtualizeCoordinateSystemElementId),
            options);
    },
    subscribeVirtualizeCoordinateSystemScrollIntoView: function (elementId, plainTextEditorGuid) {
        this.dotNetObjectReferenceByVirtualizeCoordinateSystemElementId.set(elementId, {
            intersectionRatio: 0,
            plainTextEditorGuid: plainTextEditorGuid
        });

        let element = document.getElementById(elementId);
        this.intersectionObserver.observe(element);
    },
    disposeVirtualizeCoordinateSystemScrollIntoView: function (elementId) {
        let element = document.getElementById(elementId);
        this.intersectionObserver.unobserve(element);
    },
    handleThresholdChange: function (entries, dotNetObjectReferenceByVirtualizeCoordinateSystemElementId) {
        for (let i = 0; i < entries.length; i++) {
            let currentEntry = entries[i];

            let previousValue = dotNetObjectReferenceByVirtualizeCoordinateSystemElementId.get(currentEntry.target.id);

            dotNetObjectReferenceByVirtualizeCoordinateSystemElementId.set(currentEntry.target.id, {
                intersectionRatio: currentEntry.intersectionRatio,
                plainTextEditorGuid: previousValue.plainTextEditorGuid
            });
        }
    }
};