FROM example.invalid/library/node:18-alpine

ENV NODE_OPTIONS=--openssl-legacy-provider


# Create app directory
RUN mkdir -p /usr/src/app
WORKDIR /usr/src/app
# Install app dependencies
COPY package.json /usr/src/app/
COPY yarn.lock /usr/src/app/

# 下面兩行是 2020/11 新增
# RUN apk --no-cache add g++ gcc libgcc libstdc++ linux-headers make python
# RUN npm install --quiet node-gyp -g


RUN yarn install --ignore-engines && yarn cache clean

# Bundle app source
COPY . /usr/src/app
CMD [ "yarn", "serve", "--mode", "production", "--sourcemap=false"]