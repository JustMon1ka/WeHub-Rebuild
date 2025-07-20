# 模块文件标准
```
modules
 │  README.md
 │
 ├─auth
 │  │  router.ts
 │  │  style.css
 │  │  public.ts
 │  │
 │  ├─assets
 │  │      vue.svg
 │  │
 │  ├─components
 │  │
 │  └─views
 │          LoginView.vue
 │          MeView.vue
 │          RegisterView.vue
 │
 ├─post
 ├─core
 └─user
```
如图所示，模块文件夹下包含三个子文件夹：assets、components 和 views；包含两个文件：router.ts和public.ts
- assets：存放模块相关的静态资源，如图片、字体、图标等
- components：存放模块相关的 Vue 组件
- views：存放模块相关的页面
- router.ts：模块的路由配置文件，其中包含各个页面的路由信息，在本模块写完后需要在主路由文件中引入，可以查看core和auth的router.ts文件作为模板
- public.ts：模块需要暴露的公共方法、变量或组件等，可以查看auth模块的public.ts文件（导出）和core模块的HomeView.vue文件（引入），这个只是将所有导出集中到一起，方便其他模块引入使用，原先的组件还是需要导出的。

编写时建议使用相对路径，不过可以在tsconfig.app.json中配置路径别名，如：
```json
{
  "paths": {
    "@/*": ["./src/*"],
    "@user": ["./src/user"],
    "@auth": ["./src/auth"],
    "@core": ["./src/core"]
  }
}
```

生成的HTML中使用了大量的TailwindCSS的类名，请阅读官方文档[TailwindCSS](https://tailwindcss.com/docs/installation/using-vite#vue)了解其用法。可以安装VSCode插件[Tailwind CSS IntelliSense](https://marketplace.visualstudio.com/items?itemName=bradlc.vscode-tailwindcss)来获得更好的开发体验。
> tailwindcss默认单位是rem，1rem = 16px。
> tailwindcss中文文档是旧版(v3.x)的，不建议使用，安装的是最新版本(v4.1)

生成的HTML页面中的Head标签下的内容是全局内容，已在base.css中定义

APP.vue中添加的所有组件都会被渲染到主页面中，谨慎添加！！！

> core文件夹是包含一些全局组件和页面，比如Sidebar侧边栏等。

> Sidebar的可能实现：写一个主页面，使用component :is="currentView"这一组件直接切换页面。不需要每个模块都引入sidebar。
> 但是这样要求每个模块的主页面不要写到路由中，并提供一个对应模块的主页面组件。
> 或者每个模块都引入Sidebar组件，使用<Sidebar />标签。
> 不过这个问题相对好修改。