import { ref } from 'vue';
import type { Ref } from 'vue';

const styles= ref({
  "error": "text-red-400 text-sm mt-2",
  "info": "text-sky-300 text-sm mt-2",
  "card": "bg-slate-800 p-8 rounded-2xl shadow-lg",
  "bg": "bg-slate-900 text-slate-200 flex justify-center h-full px-4",
  "label": "block text-sm font-medium text-slate-300",
  "input": "mt-1 block w-full bg-slate-700/50 border border-slate-600 rounded-md shadow-sm py-3 px-4 " +
    "focus:outline-none focus:ring-2 focus:ring-sky-500 focus:border-sky-500 sm:text-sm",
  "submitBtnShape": "w-full justify-center py-3 px-4 border border-transparent rounded-full shadow-sm " +
    "text-sm font-bold focus:outline-none focus:ring-2 focus:ring-offset-2 transition-colors duration-200 ",
  "AuthBtnShape" : "mt-1 w-2/3 py-3 px-1 border border-transparent rounded-lg shadow-sm text-sm " +
    "font-bold focus:outline-none focus:ring-2 focus:ring-offset-2 transition-colors duration-200",
  "BtnNormal": "text-white bg-sky-500 hover:bg-sky-600 focus:ring-offset-slate-900 focus:ring-sky-500 ",
  "BtnDisabled": "text-slate-400 bg-slate-700 cursor-not-allowed focus:ring-offset-slate-900 focus:ring-sky-500",
  "BtnLoading": "text-slate-400 bg-slate-700 cursor-wait focus:ring-offset-slate-900 focus:ring-sky-500",
  }
)

export default styles;
