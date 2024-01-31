FROM node:18-slim AS base
ENV ECS_AVAILABLE_LOGGING_DRIVERS='["json-file","awslogs"]'

FROM node:18 AS build
WORKDIR /src

COPY package*.json ./
COPY . .

WORKDIR /src
RUN npm install
RUN npm run build

FROM base as final
WORKDIR /app
COPY --from=build /src/dist/ .

CMD ["npm", "start"]



