import 'bootstrap';
import 'time-elements/dist/time-elements.js';
import '@github/g-emoji-element';

$(function() {
    $('a.commit-tease-sha').each(function() {
        $(this).attr('href', 'https://github.com' + $(this).attr('href'));
    })
})