document.addEventListener('DOMContentLoaded', function () {
    const form = document.getElementById('login-form');
    const fields = form.querySelectorAll('input[name="Username"], input[name="Password"]');
    const validations = {
        'Username': {
            'regex': /^[a-zA-Z0-9]{3,50}$/,
            'message': 'Username should be between 3 and 50 alphanumeric characters.',
            'required': true
        },
        'Password': {
            'regex': /^(?=.*[A-Za-z])(?=.*\d)(?=.*[@$!%*?&])[A-Za-z\d@$!%*?&]{8,}$/,
            'message': 'Password must be minimum eight characters, at least one letter and one number.',
            'required': true
        }
    };

    fields.forEach(field => {
        field.addEventListener('input', function () {
            validateField(field);
        });
    });

    form.addEventListener('submit', function (e) {
        let formIsValid = true;

        fields.forEach(field => {
            if (!validateField(field)) {
                formIsValid = false;
            }
        });

        if (!formIsValid) {
            e.preventDefault(); // Prevent form submission if validation fails
        }
    });

    function validateField(field) {
       
        const fieldName = field.name;
        const value = field.value.trim();
        const validation = validations[fieldName];
        let isValid = true;

        if (validation) {
            if (validation.required && value === '') {
                isValid = false;
                showError(field, `${fieldName} is required.`);
            } else if (validation.regex && !validation.regex.test(value)) {
                isValid = false;
                showError(field, validation.message);
            } else if (isValid) {
                showSuccess(field);
            }
        }

        return isValid;
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
