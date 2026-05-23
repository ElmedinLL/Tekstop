/** @type {import('tailwindcss').Config} */
export default {
  darkMode: 'class',
  content: ['./index.html', './src/**/*.{js,ts,jsx,tsx}'],
  theme: {
    extend: {
      ringColor: {
        brand: '#2563eb',
      },
    },
  },
  plugins: [],
}
