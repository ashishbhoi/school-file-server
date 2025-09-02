// Please see documentation at https://learn.microsoft.com/aspnet/core/client-side/bundling-and-minification
// for details on configuring this project to bundle and minify static web assets.

// Mobile Navigation Menu Toggle
document.addEventListener('DOMContentLoaded', function() {
    const mobileMenuButton = document.getElementById('mobile-menu-button');
    const mobileMenu = document.getElementById('mobile-menu');
    const menuIcon = mobileMenuButton.querySelector('svg');

    if (mobileMenuButton && mobileMenu) {
        mobileMenuButton.addEventListener('click', function() {
            const isHidden = mobileMenu.classList.contains('hidden');
            
            if (isHidden) {
                // Show menu
                mobileMenu.classList.remove('hidden');
                mobileMenu.classList.add('animate-fade-in');
                
                // Change hamburger to X icon
                menuIcon.innerHTML = `
                    <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M6 18L18 6M6 6l12 12"/>
                `;
                
                // Add overlay click handler to close menu
                document.addEventListener('click', closeMenuOnOutsideClick);
            } else {
                // Hide menu
                mobileMenu.classList.add('hidden');
                mobileMenu.classList.remove('animate-fade-in');
                
                // Change X back to hamburger icon
                menuIcon.innerHTML = `
                    <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M4 6h16M4 12h16M4 18h16"/>
                `;
                
                // Remove overlay click handler
                document.removeEventListener('click', closeMenuOnOutsideClick);
            }
        });

        // Close menu when clicking outside
        function closeMenuOnOutsideClick(event) {
            if (!mobileMenu.contains(event.target) && !mobileMenuButton.contains(event.target)) {
                mobileMenu.classList.add('hidden');
                mobileMenu.classList.remove('animate-fade-in');
                
                // Reset to hamburger icon
                menuIcon.innerHTML = `
                    <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M4 6h16M4 12h16M4 18h16"/>
                `;
                
                document.removeEventListener('click', closeMenuOnOutsideClick);
            }
        }

        // Close menu when window is resized to desktop size
        window.addEventListener('resize', function() {
            if (window.innerWidth >= 768) { // md breakpoint
                mobileMenu.classList.add('hidden');
                mobileMenu.classList.remove('animate-fade-in');
                
                // Reset to hamburger icon
                menuIcon.innerHTML = `
                    <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M4 6h16M4 12h16M4 18h16"/>
                `;
                
                document.removeEventListener('click', closeMenuOnOutsideClick);
            }
        });

        // Close menu when navigation link is clicked
        const mobileMenuLinks = mobileMenu.querySelectorAll('a');
        mobileMenuLinks.forEach(link => {
            link.addEventListener('click', function() {
                mobileMenu.classList.add('hidden');
                mobileMenu.classList.remove('animate-fade-in');
                
                // Reset to hamburger icon
                menuIcon.innerHTML = `
                    <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M4 6h16M4 12h16M4 18h16"/>
                `;
                
                document.removeEventListener('click', closeMenuOnOutsideClick);
            });
        });
    }
});

// Touch-friendly hover effects for mobile devices
document.addEventListener('DOMContentLoaded', function() {
    // Add touch support for buttons and links
    const touchElements = document.querySelectorAll('.touch-target');
    
    touchElements.forEach(element => {
        element.addEventListener('touchstart', function() {
            this.classList.add('touch-active');
        });
        
        element.addEventListener('touchend', function() {
            setTimeout(() => {
                this.classList.remove('touch-active');
            }, 150);
        });
    });
});
