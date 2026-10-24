import { dashboard } from './dashboard'

export const useLHD = () => {

    const awardTxt = (award: number) => {
        switch (award) {
            case 1:
                return "龍"
            case 2:
                return "和"
            case 3:
                return "虎"
        }
        return ""
    }

    const awardCSS = (award: number, full?: boolean) => {
        switch (award) {
            case 1:
                return full ? 
                "border-0 bg-blue-500 text-slate-100 dark:bg-blue-400 dark:text-slate-700":
                "border-blue-500 text-blue-500 dark:border-blue-400 dark:text-blue-400"
            case 2:
                return full ? 
                "border-0 bg-green-500 text-slate-100 dark:bg-green-400 dark:text-slate-700":
                "border-green-500 text-green-500 dark:border-green-400 dark:text-green-400"
            case 3:
                return full ? 
                "border-0 bg-red-500 text-slate-100 border-slate-100 dark:bg-red-400 dark:text-slate-700 dark:border-slate-700":
                "border-red-500 text-red-500 dark:border-red-400 dark:text-red-400"
            }
        return ""
    }
    
    const stateTxt = (state: number) => {
        switch (state) {
            case 0:
                return "準備中"
            case 1:
                return "下注中"
            case 2:
                return "開獎中"
            case 3:
                return "開獎結果"
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