// Smooth scroll behavior
document.addEventListener('DOMContentLoaded', function() {
    // Navbar scroll effect
    const navbar = document.querySelector('.navbar-custom');

    if (navbar) window.addEventListener('scroll', function() {
        if (window.scrollY > 50) {
            navbar.style.padding = '0.5rem 0';
            navbar.style.boxShadow = '0 2px 20px rgba(0, 0, 0, 0.1)';
        } else {
            navbar.style.padding = '1rem 0';
            navbar.style.boxShadow = '0 4px 20px rgba(0, 0, 0, 0.08)';
        }
    });

    // Smooth scroll for in-page links like "#reviews".
    // Plain "#" links (e.g. dropdown toggles) are skipped: querySelector('#') throws an error.
    document.querySelectorAll('a[href^="#"]').forEach(anchor => {
        anchor.addEventListener('click', function (e) {
            const href = this.getAttribute('href');
            if (!href || href === '#' || this.hasAttribute('data-bs-toggle')) return;

            let target = null;
            try { target = document.querySelector(href); } catch { return; }
            if (target) {
                e.preventDefault();
                target.scrollIntoView({ behavior: 'smooth', block: 'start' });
                history.replaceState(null, '', href);
            }
        });
    });

    // Animation on scroll
    const observerOptions = {
        threshold: 0.1,
        rootMargin: '0px 0px -100px 0px'
    };

    const observer = new IntersectionObserver(function(entries) {
        entries.forEach(entry => {
            if (entry.isIntersecting) {
                entry.target.style.opacity = '1';
                entry.target.style.transform = 'translateY(0)';
            }
        });
    }, observerOptions);

    // Observe elements for animation
    const animatedElements = document.querySelectorAll('.feature-card, .product-card, .testimonial-card');
    animatedElements.forEach(el => {
        el.style.opacity = '0';
        el.style.transform = 'translateY(30px)';
        el.style.transition = 'opacity 0.6s ease, transform 0.6s ease';
        observer.observe(el);
    });

    // Counter animation for stats
    const counters = document.querySelectorAll('.stat-number');
    const speed = 200; // Animation speed

    // Keeps whatever follows the number, so "50K+" stays "50K+" (it used to end as "50+")
    const animateCounter = (counter) => {
        const original = counter.innerText.trim();
        const match = original.match(/^(\d+)(.*)$/);
        if (!match) return;
        const target = parseInt(match[1], 10);
        const suffix = match[2];
        if (target === 0) return;
        const increment = target / speed;
        let count = 0;

        const updateCount = () => {
            count += increment;
            if (count < target) {
                counter.innerText = Math.ceil(count) + suffix;
                setTimeout(updateCount, 1);
            } else {
                counter.innerText = original;
            }
        };

        updateCount();
    };

    // Observe stats section
    const statsObserver = new IntersectionObserver(function(entries) {
        entries.forEach(entry => {
            if (entry.isIntersecting) {
                counters.forEach(counter => animateCounter(counter));
                statsObserver.unobserve(entry.target);
            }
        });
    }, { threshold: 0.5 });

    const statsSection = document.querySelector('.stats-section');
    if (statsSection) {
        statsObserver.observe(statsSection);
    }

    // (Removed a placeholder that cancelled every "Add to Cart" button with class
    // btn-primary-custom and showed a fake "Added to cart!" message instead.)

    // Add animations to CSS
    const style = document.createElement('style');
    style.innerHTML = `
        @keyframes slideIn {
            from {
                transform: translateX(400px);
                opacity: 0;
            }
            to {
                transform: translateX(0);
                opacity: 1;
            }
        }
        @keyframes slideOut {
            from {
                transform: translateX(0);
                opacity: 1;
            }
            to {
                transform: translateX(400px);
                opacity: 0;
            }
        }
    `;
    document.head.appendChild(style);
});
