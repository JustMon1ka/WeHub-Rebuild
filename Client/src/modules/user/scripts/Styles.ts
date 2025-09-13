import { ref } from 'vue';

const styles= ref({
    error: "text-red-400 text-sm mt-2",
    info: "text-sky-300 text-sm mt-2",
    card: "bg-slate-800 p-8 rounded-2xl shadow-lg",
    bg: "bg-slate-900 text-slate-200 flex justify-center h-full px-4",
    label: "block text-md font-medium text-slate-300",
    input: "mt-1 block w-full bg-slate-800 border border-slate-700 rounded-md shadow-sm py-2 px-3 " +
      "focus:outline-none focus:ring-sky-500 focus:border-sky-500 sm:text-sm",
    btnShape:"border-2 font-bold py-2 px-4 rounded-full transition-colors duration-200",
    submitBtn: "text-white bg-sky-500 hover:bg-sky-600 focus:ring-offset-slate-900 focus:ring-sky-500",
    normalBtn: "text-slate-400 bg-slate-700 border-slate-600 hover:bg-slate-800",
    disabledBtn: "text-slate-400 bg-slate-700 cursor-not-allowed focus:ring-offset-slate-900 focus:ring-sky-500",
    loadingBtn: "text-slate-400 bg-slate-700 cursor-wait focus:ring-offset-slate-900 focus:ring-sky-500",
    followBtnShape: "following-btn w-full font-semibold text-sm py-2 px-4 rounded-full transition-colors duration-200 ",
    followBtn: "bg-slate-200 hover:bg-white text-slate-900",
    followingBtn: "border-slate-600 border-2 hover:border-red-500/50 hover:bg-red-500/10 hover:text-red-500",
    userPic: "w-full h-full rounded-full object-cover justify-center items-center flex",
    TabFocus: "w-full text-center pt-3 pb-3.5 font-semibold border-b-2 border-sky-500",
    TabNormal: "w-full text-center pt-3 pb-3.5 text-slate-400 hover:bg-slate-800 transition-colors duration-200",
    TagBasic: "relative cursor-default text-center px-3 py-0.5 rounded-full text-2xs font-semibold text-slate-200 " +
      "border border-slate-500 border-2 transition-all duration-200 ease-in-out hover:bg-slate-700 hover:border-slate-300",
    TagEdit: "animate-shake",
    TagDelete: "absolute -top-2 -right-2 w-4.5 h-4.5 flex items-center justify-center text-xs font-bold " +
      "rounded-full bg-slate-400 text-slate-800 hover:bg-red-500 hover:text-slate-50",
  }
)

export default styles;
