import { dashboard } from './dashboard'

export const useCJLHLP = () => {

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
            // 0 綠色
            case 0:
                return full ? 
                "border-0 bg-emerald-500 text-emerald-100 dark:bg-emerald-400 dark:text-emerald-700":
                "border-emerald-500 text-emerald-500 dark:border-emerald-400 dark:text-emerald-400"
            // 紅色
            case 1:
            case 3:
            case 5:
            case 7:
            case 9:
            case 12:
            case 14:
            case 16:
            case 18:
            case 19:
            case 21:
            case 23:
            case 25:
            case 27:
            case 30:
            case 32:
            case 34:
            case 36:
                return full ? 
                "border-0 bg-rose-400 text-rose-100 dark:bg-rose-300 dark:text-rose-700":
                "border-rose-400 text-rose-400 dark:border-rose-300 dark:text-rose-400"
            // 黑色
            case 2:
            case 4:
            case 6:
            case 8:
            case 10:
            case 11:
            case 13:
            case 15:
            case 17:
            case 20:
            case 22:
            case 24:
            case 26:
            case 28:
            case 29:
            case 31:
            case 33:
            case 35:
                return full ? 
                "border-0 bg-neutral-800 text-neutral-100 dark:bg-neutral-400 dark:text-neutral-700":
                "border-neutral-500 text-neutral-500 dark:border-neutral-400 dark:text-neutral-400"
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