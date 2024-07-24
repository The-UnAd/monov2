# NOTE: this Dockerfile is intended to be built from the root of the project
FROM node:18-alpine AS builder
WORKDIR /app

COPY package.json package-lock.json* ./
COPY web/admin-ui/package.json ./web/admin-ui/
RUN npm ci

COPY web/admin-ui ./web/admin-ui
WORKDIR /app/web/admin-ui
RUN npm run build

FROM nginx:1.27-alpine AS runner

RUN rm /etc/nginx/conf.d/default.conf

COPY web/admin-ui/admin-ui.nginx.conf /etc/nginx/conf.d/default.conf
COPY --from=builder /app/web/admin-ui/build /usr/share/nginx/html
ENTRYPOINT ["nginx", "-g", "daemon off;"]
