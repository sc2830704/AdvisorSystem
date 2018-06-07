function jsonStringFilter(jsonString) {
    return jsonString.replace(/\n/g, "\\n").replace(/\r/g, "\\r").replace(/\t/g, "\\t").replace(/&quot;/g, '\"');
}

$.fn.bootstrapBtn = $.fn.button.noConflict();