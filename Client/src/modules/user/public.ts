
/* Component: PlaceHolder是用来生成默认用户头像的组件。
 * props: width :number - svg宽度
 *        height :number - svg高度
 *        text :string - 显示的文字内容，只显示首字母，支持中文
 *        bgColor :string - 背景颜色，十六进制颜色值， 如：#123456
 *        textColor :string - 文字颜色
 */
export {default as PlaceHolder} from './components/PlaceHolder.vue';


/* Component: UserList是用户列表组件，显示用户信息的列表
 * props:  generator: syncGenerator<Array<string>, string, number>
 *                    生成器函数，用于模拟加载用户数据，yield 返回用户ID列表，每次调用传入一个数字表示请求的数量，
 *                    return返回一个字符串，非空时表示加载失败，值表示错误信息，
 *                    初始时会调用一次next()用于初始化，此时返回空数组即可，后续调用next()再传入数字表示请求的用户数量
 *                    样例参见 FollowList.vue 第 37 行 的generator的定义
 *         followBtn :boolean - 是否显示关注按钮，可以缺省，默认不显示
 */
export { default as UserList } from './components/UserList/UserList.vue';


/* Component: UserCard是用户卡片组件，显示用户信息的卡片列表，自带用户悬浮窗
 * props: userId :string - 用户ID
 *        followBtn :boolean - 是否显示关注按钮，可以缺省，默认不显示
 * slot:  有一个名为content的插槽，可以用来放置自定义内容，比如帖子文章等。
 *        默认内容是用户简介。
 * events: loaded - 当用户信息加载完成时触发，事件参数为用户信息对象
 */
export { default as UserCard} from './components/UserList/UserCardList.vue';


/* class UserInfo是用户信息类，用于获取和管理用户信息
 * 通过构造函数传入用户ID，获取用户信息。除关注用户数量外，所有信息均来自userProfile表
 *  attributes: userId :string - 用户ID
 *              nickName :string - 昵称
 *              profilePictureUrl: string = '' 用户资料图片URL
 *              userAvatarUrl: string = '' 用户头像URL
 *              birthday: string = '' 用户生日，格式为YYYY-MM-DD
 *              level: number = 0 用户等级
 *              address: string = '' 用户地址
 *              bio: string = '' 用户简介
 *              isMale: boolean = true 用户性别，true表示男性，false表示女
 *              followerCount: number = 0 关注者数量
 *              followingCount: number = 0 关注用户数量
 *              isMe: boolean = false 当前类实例是否是当前登录用户
 * 其他属性为 UI 相关属性，主要用于用户信息编辑界面
 */
export { default as UserInfo } from './scripts/UserInfo.ts';
