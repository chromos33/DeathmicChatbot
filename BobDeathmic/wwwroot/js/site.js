// Write your JavaScript code.
jQuery(".submenuopener").click(function () {
    if (jQuery(this).closest(".menuitem").find(".submenu").height() === 0) {
        jQuery(this).closest(".menuitem").addClass("open");
        jQuery(this).closest(".menuitem").find(".submenu").height(jQuery(this).closest(".menuitem").find(".heightgiver").outerHeight());
        ChangeMobileHeight(jQuery(this).closest(".menuitem").find(".heightgiver").outerHeight());
    } else {
        jQuery(this).closest(".menuitem").find(".submenu").height(0);
        jQuery(this).closest(".menuitem").removeClass("open");
        ChangeMobileHeight(-jQuery(this).closest(".menuitem").find(".heightgiver").outerHeight());
    }
    
});
function ChangeMobileHeight(value) {
    if (jQuery(window).width() < 768) {
        jQuery(".mobile_overflowcontainer").height(jQuery(".mobile_overflowcontainer").height() + value);
    }
}
jQuery(".mobile_nav_opener").click(function () {
    if (jQuery(".mobile_overflowcontainer").height() == 0) {
        jQuery(".mobile_overflowcontainer").height(jQuery(".mobile_overflowcontainer > div").outerHeight());
    }
    else {
        jQuery(".mobile_overflowcontainer").height(0);
    }
});

jQuery(document).ready(function () {
    jQuery(window).on("resize", handleMobileSwitch);
    jQuery(window).ready(handleMobileSwitch);
    function handleMobileSwitch() {
        if (jQuery(window).width() < 768) {
            jQuery(".mobile_overflowcontainer").height(0);

        }
        else {
            jQuery(".mobile_overflowcontainer").css("height", "auto");
        }
    }
})