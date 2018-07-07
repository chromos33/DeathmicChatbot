// Write your JavaScript code.
jQuery(".submenuopener").click(function () {
    if (jQuery(this).closest(".menuitem").find(".submenu").height() === 0) {
        jQuery(this).closest(".menuitem").addClass("open");
        jQuery(this).closest(".menuitem").find(".submenu").height(jQuery(this).closest(".menuitem").find(".heightgiver").outerHeight());
    } else {
        jQuery(this).closest(".menuitem").find(".submenu").height(0);
        jQuery(this).closest(".menuitem").removeClass("open");
    }
    
});