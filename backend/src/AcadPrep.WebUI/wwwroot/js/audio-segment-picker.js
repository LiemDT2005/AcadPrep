(function () {
    function formatTime(seconds) {
        const s = Math.max(0, Math.round(seconds));
        const m = Math.floor(s / 60);
        const r = s % 60;
        return m + ':' + r.toString().padStart(2, '0');
    }

    function removeAllRegions(wavesurfer) {
        if (!wavesurfer.regions || !wavesurfer.regions.list) return;
        Object.keys(wavesurfer.regions.list).forEach(function (id) {
            wavesurfer.regions.list[id].remove();
        });
    }

    window.initAudioSegmentPicker = function (config) {
        if (typeof WaveSurfer === 'undefined') {
            console.error('WaveSurfer is not loaded.');
            return null;
        }

        var pickerId = config.pickerId;
        var waveContainer = document.getElementById(pickerId + '-wave');
        var loadingEl = document.getElementById(pickerId + '-loading');
        var controlsEl = document.getElementById(pickerId + '-controls');
        var timeDisplay = document.getElementById(pickerId + '-time-display');
        var startInput = document.querySelector('input[name="' + config.startInputName + '"]');
        var endInput = document.querySelector('input[name="' + config.endInputName + '"]');

        if (!waveContainer || !startInput || !endInput) {
            return null;
        }

        function updateDisplay(start, end) {
            var startSec = Math.round(start);
            var endSec = Math.round(end);
            startInput.value = startSec;
            endInput.value = endSec;
            if (timeDisplay) {
                timeDisplay.textContent = formatTime(startSec) + ' → ' + formatTime(endSec) + ' (' + (endSec - startSec) + 's)';
            }
            if (wavesurfer.isPlaying()) {
                wavesurfer.pause();
            }
            document.dispatchEvent(new CustomEvent('audioSegmentChanged', {
                detail: { start: startSec, end: endSec, pickerId: pickerId }
            }));
        }

        var wavesurfer = WaveSurfer.create({
            container: waveContainer,
            waveColor: '#c7c4d8',
            progressColor: '#3525cd',
            cursorColor: '#1e00a9',
            barWidth: 2,
            barGap: 1,
            barRadius: 2,
            height: 100,
            minPxPerSec: 2,
            scrollParent: true,
            hideScrollbar: false,
            backend: 'MediaElement',
            plugins: [
                WaveSurfer.regions.create({
                    dragSelection: false
                })
            ]
        });

        var activeRegion = null;
        var currentZoom = 2;

        function createRegion(duration) {
            removeAllRegions(wavesurfer);

            var start = config.initialStart != null ? config.initialStart : parseInt(startInput.value, 10);
            var end = config.initialEnd != null ? config.initialEnd : parseInt(endInput.value, 10);

            if (isNaN(start)) start = 0;
            if (isNaN(end) || end <= start) end = Math.min(start + 10, duration);

            start = Math.max(0, Math.min(start, duration - 1));
            end = Math.max(start + 1, Math.min(end, duration));

            activeRegion = wavesurfer.addRegion({
                start: start,
                end: end,
                color: 'rgba(30, 0, 169, 0.25)',
                drag: true,
                resize: true
            });

            updateDisplay(activeRegion.start, activeRegion.end);
        }

        wavesurfer.on('ready', function () {
            if (loadingEl) loadingEl.classList.add('hidden');
            waveContainer.classList.remove('hidden');
            if (controlsEl) controlsEl.classList.remove('hidden');
            createRegion(wavesurfer.getDuration());
        });

        wavesurfer.on('error', function (err) {
            if (loadingEl) {
                loadingEl.textContent = 'Could not load waveform. Use the time inputs below.';
            }
            console.error(err);
        });

        wavesurfer.on('region-update-end', function (region) {
            updateDisplay(region.start, region.end);
        });

        wavesurfer.load(config.audioUrl);

        var playBtn = document.getElementById(pickerId + '-play-segment');

        function setPlayButtonState(playing) {
            if (!playBtn) return;
            var icon = playBtn.querySelector('.material-symbols-outlined');
            var label = playBtn.querySelector('[data-toggle-label]');
            if (icon) icon.textContent = playing ? 'pause' : 'play_arrow';
            if (label) label.textContent = playing ? 'Pause' : 'Play segment';
        }

        wavesurfer.on('play', function () { setPlayButtonState(true); });
        wavesurfer.on('pause', function () { setPlayButtonState(false); });
        wavesurfer.on('finish', function () { setPlayButtonState(false); });

        if (playBtn) {
            playBtn.addEventListener('click', function () {
                if (wavesurfer.isPlaying()) {
                    wavesurfer.pause();
                } else if (activeRegion) {
                    activeRegion.play();
                }
            });
        }

        var zoomInBtn = document.getElementById(pickerId + '-zoom-in');
        if (zoomInBtn) {
            zoomInBtn.addEventListener('click', function () {
                currentZoom = Math.min(currentZoom * 2, 200);
                wavesurfer.zoom(currentZoom);
            });
        }

        var zoomOutBtn = document.getElementById(pickerId + '-zoom-out');
        if (zoomOutBtn) {
            zoomOutBtn.addEventListener('click', function () {
                currentZoom = Math.max(currentZoom / 2, 0.5);
                wavesurfer.zoom(currentZoom);
            });
        }

        function syncRegionFromInputs() {
            if (!activeRegion || !wavesurfer.isReady) return;
            var start = parseFloat(startInput.value);
            var end = parseFloat(endInput.value);
            var duration = wavesurfer.getDuration();
            if (isNaN(start) || isNaN(end) || end <= start || end > duration) return;

            removeAllRegions(wavesurfer);
            activeRegion = wavesurfer.addRegion({
                start: start,
                end: end,
                color: 'rgba(30, 0, 169, 0.25)',
                drag: true,
                resize: true
            });
            updateDisplay(start, end);
            wavesurfer.seekTo(start / duration);
        }

        startInput.addEventListener('change', syncRegionFromInputs);
        endInput.addEventListener('change', syncRegionFromInputs);

        return wavesurfer;
    };
})();
