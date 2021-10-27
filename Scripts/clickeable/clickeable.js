function runClickeable() {

    $(".clickeable").click(function () {
        var href = this.dataset.href;
        if (href) {
            window.location.assign(href);
        }
    });

}