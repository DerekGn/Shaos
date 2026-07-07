const eventSource = new EventSource('/api/v1/log-events');
const trace = document.getElementById('trace');
const maxTraceLines = 100;

window.onload = function () {
    eventSource.addEventListener('log-created-event', (event) => {
        console.debug('log-created-event');

        if (trace != null) {
            const traceLines = trace.value.split('\n');
            const logEvent = JSON.parse(event.data);
            const newTraceLines = logEvent.log.split('\r\n');

            if (traceLines.length > maxTraceLines) {

                var count = 0;

                for (var i = 0; i < newTraceLines.length; i++) {
                    count += traceLines[i].length + 1;
                }

                console.debug("Trace Lines: [" + traceLines.length + "] New Trace Lines: [" + newTraceLines.length + "] Count: [" + count + "]");

                trace.value = trace.value.slice(count);
            }

            trace.value += logEvent.log;

            trace.setSelectionRange(trace.value.length, trace.value.length);
        }
        else {
            console.warn("trace is null");
        }
    });

    eventSource.onerror = (err) => {
        console.error('EventSource failed:', err);
        eventSource.close();
    };

    eventSource.onopen = () => {
        console.log('Connection to server opened.');
    };
}