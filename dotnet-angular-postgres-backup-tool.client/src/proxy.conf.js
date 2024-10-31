const PROXY_CONFIG = [
  {
    context: [
      "/api/Backup",
      "/api/Backup/latest"
    ],
    target: "https://localhost:7298",
    secure: false
  }
]

module.exports = PROXY_CONFIG;
