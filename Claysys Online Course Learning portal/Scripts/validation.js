document.addEventListener('DOMContentLoaded', function () {
    const form = document.getElementById('signup-form');
    const fields = form.querySelectorAll('input, textarea, select');
    const validations = {
        'FirstName': {
            'regex': /^[a-zA-Z]{2,50}$/,
            'message': 'First name should be between 3 and 50 letters.'
        },
        'LastName': {
            'regex': /^[a-zA-Z]{2,50}$/,
            'message': 'Last name should be between 2 and 50 letters.'
        },
        'Email': {
            'regex': /^[^\s@]+@[^\s@]+\.[^\s@]+$/,
            'message': 'Enter a valid email address.',
            'ajax': '/Account/CheckEmail'
        },
        'Phone': {
            'regex': /^[0-9]{10}$/,
            'message': 'Phone number should be exactly 10 digits.'
        },
        'Password': {
            'regex': /^(?=.*[A-Za-z])(?=.*\d)(?=.*[@$!%*?&])[A-Za-z\d@$!%*?&]{8,}$/,
            'message': 'Password must be minimum eight characters, at least one letter and one number.'
        },
        'ConfirmPassword': {
            'match': 'Password',
            'message': 'Passwords do not match.'
        },
        'Username': {
            'regex': /^[a-zA-Z0-9]{3,50}$/,
            'message': 'Username should be between 3 and 50 alphanumeric characters.',
            'ajax': '/Account/CheckUsername'
        }
    };

    fields.forEach(field => {
        field.addEventListener('input', function () {
            validateField(field);
        });
    });

    function validateField(field) {
        const fieldName = field.name;
        const value = field.value;
        const validation = validations[fieldName];

        if (validation) {
            let isValid = true;

            if (validation.regex && !validation.regex.test(value)) {
                isValid = false;
                showError(field, validation.message);
            } else if (validation.match) {
                const matchValue = form.querySelector(`input[name="${validation.match}"]`).value;
                if (value !== matchValue) {
                    isValid = false;
                    showError(field, validation.message);
                }
            } else if (validation.ajax) {
                checkAvailability(field, validation.ajax);
                return;
            }

            if (isValid) {
                showSuccess(field);
            }
        }
    }

    function checkAvailability(field, url) {
        const value = field.value;
        $.ajax({
            url: url,
            type: 'POST',
            data: { value: value },
            success: function (response) {
                if (!response.available) {
                    showError(field, `${field.name} is already taken.`);
                } else {
                    showSuccess(field);
                }
            },
            error: function () {
                showError(field, 'Server error, please try again later.');
            }
        });
    }

    function showError(field, message) {
        field.classList.remove('input-success');
        field.classList.add('input-error');
        let error = field.nextElementSibling;
        if (!error || !error.classList.contains('error-message')) {
            error = document.createElement('div');
            error.classList.add('error-message');
            field.parentNode.insertBefore(error, field.nextSibling);
        }
        error.textContent = message;
    }

    function showSuccess(field) {
        field.classList.remove('input-error');
        field.classList.add('input-success');
        let error = field.nextElementSibling;
        if (error && error.classList.contains('error-message')) {
            error.remove();
        }
    }
});
