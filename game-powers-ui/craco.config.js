module.exports = {
    reactScriptsVersion: '@principlestudios/react-scripts-lib',
    style: {
      postcss: {
        plugins: [
          require('tailwindcss')('src/tailwind.config.js'),
          require('autoprefixer'),
        ],
      },
    },
  }
