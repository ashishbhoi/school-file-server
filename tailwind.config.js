/** @type {import('tailwindcss').Config} */
module.exports = {
    content: ["./Views/**/*.cshtml", "./Views/**/*.html", "./wwwroot/js/**/*.js"],
    theme: {
        extend: {
            fontFamily: {
                sans: ["Inter", "system-ui", "sans-serif"],
            },
        },
    },
    plugins: [require("@tailwindcss/forms"), require("@tailwindcss/typography")],
};
