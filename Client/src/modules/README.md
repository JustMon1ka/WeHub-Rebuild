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
- assets：存放模块相关的静态资源，如图片、字体、图标等，图标的svg存放在src/assets下，可以通过`<img src="@/assets/vue.svg" alt="Vue Logo">`来引用。
- components：存放模块相关的 Vue 组件
- views：存放模块相关的页面
- router.ts：模块的路由配置文件，其中包含各个页面的路由信息，在本模块写完后需要在主路由文件中引入，可以查看`src/router.ts`(主路由)和`@auth/router.ts`文件作为模板
  - router的每个路由对象需要包含以下属性：
    - path：路由路径。将模块路由添加到主路由后，就可以通过"URL/path"来访问对应的页面。
    - name：路由名称
    - component：对应的 Vue 页面
    - meta：路由元信息，包含标题、是否显示全局组件（Navigation和Recommend）。标题(meta.title)会在页面顶端显示为"title - WeHub"的形式。navi为true时，表示该页面需要显示导航栏，recommend为true时，表示该页面需要显示推荐栏。以上两值默认为false。
- public.ts：模块需要暴露的公共方法、变量或组件等，可以查看auth模块的public.ts文件（导出）和core模块的HomeView.vue文件（引入），这个只是将所有导出集中到一起，方便其他模块引入使用，原先的组件还是需要导出的。

可以在tsconfig.app.json中配置路径别名，如：
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

生成的HTML中使用了大量的TailwindCSS的类名，请阅读官方文档[TailwindCSS](https://tailwindcss.com/docs/installation/using-vite#vue)了解其用法。可以安装VSCode插件[Tailwind CSS IntelliSense](https://marketplace.visualstudio.com/items?itemName=bradlc.vscode-tailwindcss)，VSCode中自动补全可使用[TailWind auto Completion](https://www.javascriptcn.com/post/672c7e5cddd3a70eb6d85e81)，可以查看每个class对应的元素css代码。JB中似乎没有类似的插件。
> tailwindcss默认单位是rem，1rem = 16px。
> tailwindcss中文文档是旧版(v3.x)的，安装的是最新版本(v4.1)，但是大部分类名和用法是相同的。

生成的HTML页面中的Head标签下的内容是全局内容，已在base.css中定义，不需要重复添加。
```html
<!DOCTYPE html>
<html lang="zh-CN">
<head>
    <meta charset="UTF-8">
    <meta name="viewport" content="width=device-width, initial-scale=1.0">
    <title>社交论坛私信页预览</title> <!-- 页面标题，此处需要自行更改 -->
    <!-- 引入 Tailwind CSS -->
    <script src="https://cdn.tailwindcss.com"></script>
    <!-- 引入 Google Fonts 的 Inter 字体 -->
    <link rel="preconnect" href="https://fonts.googleapis.com">
    <link rel="preconnect" href="https://fonts.gstatic.com" crossorigin>
    <link href="https://fonts.googleapis.com/css2?family=Inter:wght@400;500;600;700&display=swap" rel="stylesheet">
    <style>
        /* 应用 Inter 字体和自定义滚动条样式 */
        body {
            font-family: 'Inter', sans-serif;
        }
        /* 为 Webkit 浏览器（Chrome, Safari）美化滚动条 */
        ::-webkit-scrollbar {
            width: 8px;
        }
        ::-webkit-scrollbar-track {
            background: #1e293b; /* slate-800 */
        }
        ::-webkit-scrollbar-thumb {
            background: #475569; /* slate-600 */
            border-radius: 4px;
        }
        ::-webkit-scrollbar-thumb:hover {
            background: #64748b; /* slate-500 */
        }
        /* 避免在预览中出现不必要的滚动条 */
        html, body {
            overflow-x: hidden;
        }
        /* 主内容区高度，确保聊天界面撑满 */
        .main-content-height {
            height: calc(100vh - 0px); /* 减去顶部任何固定元素的高度 */
        }
    </style>
</head>
```
APP.vue中添加的所有组件都会被渲染到主页面中，谨慎添加！！！

> core文件夹是包含一些全局组件和页面，比如Sidebar侧边栏等。

> 问题： 