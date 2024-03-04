FROM node:18-alpine AS builder
WORKDIR /app

COPY package.json package-lock.json* ./
COPY web/admin-ui ./web/admin-ui
RUN npm ci

WORKDIR /app/web/admin-ui
RUN npm run build

FROM nginx:1.25-alpine AS runner

RUN rm /etc/nginx/conf.d/default.conf

COPY admin-ui.nginx.conf /etc/nginx/conf.d/default.conf
COPY --from=builder /app/web/admin-ui/build /usr/share/nginx/html
ENTRYPOINT ["nginx", "-g", "daemon off;"]
