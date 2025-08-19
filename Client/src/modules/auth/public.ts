
// 登录表单
export { default as loginForm } from './components/LoginForm.vue'

/* Class: User指代当前用户，是单例类
 * 该类包含登录、登出、注册等方法，还负则管理登录的cookie
 * 该类的实例可以通过User.getInstance()获取
 * 通过该实例的userAuth方法（为存取器）获取当前用户的userId和token
 * 该类还包含followingList和followedList属性，表示当前用户关注的用户、关注当前用户的用户列表
 * 该类还包含userInfo属性，表示当前用户的信息
 */
export { default as User } from './scripts/User.js'
