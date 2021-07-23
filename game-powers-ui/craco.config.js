module.exports = {
    reactScriptsVersion: '@principlestudios/react-scripts-lib',
    style: {
      postcss: {
        plugins: [
          require('tailwindcss'),
          require('autoprefixer'),
        ],
      },
    },
  }
