"use strict";

const eventSource = new EventSource('/api/v1/events');

window.onload = function () {

    eventSource.onmessage = (event) => {
        console.log('Message');
    };

    eventSource.onerror = (err) => {
        console.error('EventSource failed:', err);
        eventSource.close();
    };

    eventSource.onopen = () => {
        console.log('Connection to server opened.');
    };
}
