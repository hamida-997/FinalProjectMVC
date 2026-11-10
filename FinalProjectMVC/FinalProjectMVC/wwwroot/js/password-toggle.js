// Adds a show/hide "eye" button to every password field that doesn't already have one
// (Login, Register and admin Change Password have their own buttons and are skipped).
document.addEventListener('DOMContentLoaded', function () {
    document.querySelectorAll('input[type="password"]').forEach(function (input) {
        const parent = input.parentElement;
        if (!parent || parent.querySelector('.password-toggle')) return;

        // Wrap the input so the button can sit inside it on the right
        const wrapper = document.createElement('div');
        wrapper.className = 'pw-toggle-wrap';
        parent.insertBefore(wrapper, input);
        wrapper.appendChild(input);

        const button = document.createElement('button');
        button.type = 'button';
        button.className = 'password-toggle';
        button.setAttribute('aria-label', 'Show password');
        button.innerHTML = '<i class="fas fa-eye"></i>';
        wrapper.appendChild(button);

        button.addEventListener('click', function () {
            const show = input.type === 'password';
            input.type = show ? 'text' : 'password';
            button.querySelector('i').className = show ? 'fas fa-eye-slash' : 'fas fa-eye';
            button.setAttribute('aria-label', show ? 'Hide password' : 'Show password');
            input.focus();
        });
    });
});
