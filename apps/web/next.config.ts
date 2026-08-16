import type { NextConfig } from "next";

const nextConfig: NextConfig = {
  output: "standalone",
  poweredByHeader: false,
  reactStrictMode: true,
  images: {
    localPatterns: [{ pathname: "/api/media/**" }],
  },
};

export default nextConfig;
