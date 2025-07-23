document.addEventListener('DOMContentLoaded', function () {
    const sidebar = document.querySelector('.sidebar');
    const mobileNavToggle = document.querySelector('.mobile-nav-toggle');
    const navLinks = document.querySelectorAll('.sidebar a');
    const sections = document.querySelectorAll('.content section');

    // Mobile navigation toggle
    mobileNavToggle.addEventListener('click', function() {
        sidebar.classList.toggle('active');
        mobileNavToggle.classList.toggle('active');
    });

    // Close sidebar when a link is clicked on mobile
    navLinks.forEach(link => {
        link.addEventListener('click', function() {
            if (window.innerWidth <= 768) {
                sidebar.classList.remove('active');
                mobileNavToggle.classList.remove('active');
            }
        });
    });

    // Active link highlighting on scroll
    function onScroll() {
        let currentSection = '';
        sections.forEach(section => {
            const sectionTop = section.offsetTop;
            if (pageYOffset >= sectionTop - 60) {
                currentSection = section.getAttribute('id');
            }
        });

        navLinks.forEach(link => {
            link.classList.remove('active');
            if (link.getAttribute('href').substring(1) === currentSection) {
                link.classList.add('active');
            }
        });
    }

    window.addEventListener('scroll', onScroll);
    onScroll(); // Initial check
});
