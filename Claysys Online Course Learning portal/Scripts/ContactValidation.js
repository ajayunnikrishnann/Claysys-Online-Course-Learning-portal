document.addEventListener('DOMContentLoaded', function () {
    const form = document.getElementById('contactForm');

    // Regular expressions for validation
    const namePattern = /^[A-Za-z\s]{3,50}$/; // Only letters and spaces, 3-50 characters
    const emailPattern = /^[^@\s]+@[^@\s]+\.[^@\s]+$/; // Basic email format
    const phonePattern = /^\d{10}$/; // Exactly 10 digits
    const messagePattern = /^.{10,500}$/; // Between 10 and 500 characters

    // Validate input and show/hide error messages
    function validateInput(input, pattern, errorElement) {
        if (pattern.test(input.value)) {
            errorElement.textContent = '';
            return true;
        } else {
            errorElement.textContent = 'Invalid input.';
            return false;
        }
    }

    // Event listeners for live validation
    document.getElementById('username').addEventListener('input', function () {
        validateInput(this, namePattern, document.getElementById('usernameError'));
    });

    document.getElementById('email').addEventListener('input', function () {
        validateInput(this, emailPattern, document.getElementById('emailError'));
    });

    document.getElementById('phone').addEventListener('input', function () {
        validateInput(this, phonePattern, document.getElementById('phoneError'));
    });

    document.getElementById('message').addEventListener('input', function () {
        validateInput(this, messagePattern, document.getElementById('messageError'));
    });

    form.addEventListener('submit', function (event) {
        let isValid = true;

        // Validate all fields on form submission
        isValid &= validateInput(document.getElementById('username'), namePattern, document.getElementById('usernameError'));
        isValid &= validateInput(document.getElementById('email'), emailPattern, document.getElementById('emailError'));
        isValid &= validateInput(document.getElementById('phone'), phonePattern, document.getElementById('phoneError'));
        isValid &= validateInput(document.getElementById('message'), messagePattern, document.getElementById('messageError'));

        // Prevent form submission if any field is invalid
        if (!isValid) {
            event.preventDefault();
        }
    });
});
