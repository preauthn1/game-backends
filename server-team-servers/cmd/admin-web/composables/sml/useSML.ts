import { dashboard } from './dashboard'

export const useSML = () => {

    const awardTxt = (award: number) => {
        switch (award) {
            case 16:
                return "進"
            case 17:
                return "左"
            case 18:
                return "中"
            case 19:
                return "右"
            case 32:
                return "守"
            case 33:
                return "左"
            case 34:
                return "中"
            case 35:
                return "右"
        }
        return ""
    }

    const awardCSS = (award: number, full?: boolean) => {
        switch (award) {
            case 16:
                return full ? 
                "border-0 bg-blue-500 text-slate-100 dark:bg-blue-400 dark:text-slate-700":
                "border-blue-500 text-blue-500 dark:border-blue-400 dark:text-blue-400"
            case 17:
                return full ? 
                "border-0 bg-blue-400 text-slate-100 dark:bg-blue-300 dark:text-slate-700":
                "border-blue-400 text-blue-400 dark:border-blue-300 dark:text-blue-400"
            case 18:
                return full ? 
                "border-0 bg-blue-500 text-slate-100 dark:bg-blue-400 dark:text-slate-700":
                "border-blue-500 text-blue-500 dark:border-blue-400 dark:text-blue-400"
            case 19:
                return full ? 
                "border-0 bg-blue-600 text-slate-100 dark:bg-blue-500 dark:text-slate-700":
                "border-blue-600 text-blue-600 dark:border-blue-500 dark:text-blue-400"
            case 32:
                return full ? 
                "border-0 bg-red-500 text-slate-100 border-slate-100 dark:bg-red-400 dark:text-slate-700 dark:border-slate-700":
                "border-red-500 text-red-500 dark:border-red-400 dark:text-red-400"
            case 33:
                return full ? 
                "border-0 bg-red-400 text-slate-100 border-slate-100 dark:bg-red-300 dark:text-slate-700 dark:border-slate-700":
                "border-red-400 text-red-400 dark:border-red-300 dark:text-red-300"
            case 34:
                return full ? 
                "border-0 bg-red-500 text-slate-100 border-slate-100 dark:bg-red-400 dark:text-slate-700 dark:border-slate-700":
                "border-red-500 text-red-500 dark:border-red-400 dark:text-red-400"
            case 35:
                return full ? 
                "border-0 bg-red-600 text-slate-100 border-slate-100 dark:bg-red-500 dark:text-slate-700 dark:border-slate-700":
                "border-red-600 text-red-600 dark:border-red-500 dark:text-red-500"
            }
        return ""
    }
    
    const stateTxt = (state: number) => {
        switch (state) {
            case 0:
                return "準備中"
            case 1:
                return "遊戲開始表演動畫"
            case 2:
                return "等待下注"
            case 3:
                return "開獎動畫"
            case 4:
                return "遊戲結束"
        }
        return ""
    }

    return {
        dashboard,
        awardTxt,
        awardCSS,
        stateTxt,
    }
}