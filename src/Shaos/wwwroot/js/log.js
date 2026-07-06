const eventSource = new EventSource('/api/v1/log-events');
const trace = document.getElementById('trace');

window.onload = function () {
    eventSource.addEventListener('log-created-event', (event) => {
        console.debug('log-created-event');

        if (trace != null) {
            var div = document.createElement("div");
            div.innerHTML = event.data;

            trace.appendChild(div);
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