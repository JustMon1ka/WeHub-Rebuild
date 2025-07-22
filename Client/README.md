# Client

This template should help get you started developing with Vue 3 in Vite.

## Recommended IDE Setup

[VSCode](https://code.visualstudio.com/) + [Volar](https://marketplace.visualstudio.com/items?itemName=Vue.volar) (and disable Vetur).

## Type Support for `.vue` Imports in TS

TypeScript cannot handle type information for `.vue` imports by default, so we replace the `tsc` CLI with `vue-tsc` for type checking. In editors, we need [Volar](https://marketplace.visualstudio.com/items?itemName=Vue.volar) to make the TypeScript language service aware of `.vue` types.

## Customize configuration

See [Vite Configuration Reference](https://vite.dev/config/).

## Project Setup

```sh
npm install
```

### Compile and Hot-Reload for Development

```sh
npm run dev
```
这条指令是用来测试的，会启动一个本地服务器，监听代码的变化并自动刷新浏览器。

### Type-Check, Compile and Minify for Production

```sh
npm run build
```
这条指令是用来发布的，会将代码编译成生产环境的版本。

### Lint with [ESLint](https://eslint.org/)

```sh
npm run lint
```
这条指令是用来检查代码规范的，会检查代码是否符合 ESLint 的规则。