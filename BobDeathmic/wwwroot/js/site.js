// Write your JavaScript code.
jQuery(".submenuopener").click(function () {
    if (jQuery(this).closest(".menuitem").find(".submenu").height() === 0) {
        jQuery(".submenuopener").closest(".menuitem").removeClass("open");
        jQuery(this).closest(".menuitem").addClass("open");
        jQuery(this).closest(".menuitem").find(".submenu").height(jQuery(this).closest(".menuitem").find(".heightgiver").outerHeight());
    } else {
        jQuery(this).closest(".menuitem").find(".submenu").height(0);
        jQuery(".submenuopener").closest(".menuitem").removeClass("open");
    }
    
});