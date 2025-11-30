# Build stage
FROM node:20-alpine AS build
WORKDIR /app

# Copy package files
COPY package*.json ./
RUN npm ci

# Copy source code and build
COPY . .
RUN npm run build

# Production stage - use serve instead of nginx
FROM node:20-alpine
WORKDIR /app

# Install serve globally
RUN npm install -g serve

# Copy build files
COPY --from=build /app/build ./build

# Expose port
EXPOSE 3000

# Run serve
CMD ["serve", "-s", "build", "-l", "3000"]