window.timezoneHelper = {
    getTimeZone: function () {
        return Intl.DateTimeFormat().resolvedOptions().timeZone;
    }
};
