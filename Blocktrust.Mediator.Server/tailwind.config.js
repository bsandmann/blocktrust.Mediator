/** @type {import('tailwindcss').Config} */
module.exports = {
    content: [
        './Pages/**/*.cshtml',
        './Views/**/*.chstml'
    ],
    theme: {
        screens: {
            'sm': '640px',
            // => @media (min-width: 640px) { ... }

            'md': '768px',
            // => @media (min-width: 768px) { ... }

            'lg': '1024px',
            // => @media (min-width: 1024px) { ... }

            'xl': '1280px',
            // => @media (min-width: 1280px) { ... }
        },
        fontFamily: {
            'museo': ['Museo', 'Helvetica', 'Arial', 'sans-serif'],
        },
        colors: {
            secondary: '#ff8f00',
            secondaryLight: '#ffc046',
            secondaryDark: '#c56000',
            primary: '#37474f',
            primaryLight: '#62727b',
            primaryDark: '#102027',
            error: '#f44336',
            errorDark: '#c21b15',
            success: '#43a047',
            myBackground: '#f4f4f4',
            blueDark: '#181658',
            blueViolett: '#342a83',
            transparent: 'transparent',
            current: 'currentColor',
            white: '#ffffff'
        },
    },
    plugins: [
        require('@tailwindcss/aspect-ratio'),
    ],
}
