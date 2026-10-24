import dayjs from "dayjs"

import { Notification } from "~/@types/notification"

export const useNotification = () => {

    const state = useState<Notification[]>("notifications", () => [])

    const remove = (key?: number) => {
        if (!key) {
            return
        }
        const indexToDelete = state.value.findIndex( n => n.key === key )
        if (indexToDelete !== -1) {
            state.value.splice(indexToDelete, 1);
        }
    }

    const notify = (item: Notification) => {

        const nItem = {
            key: dayjs().valueOf(),
            title: item.title,
            text: item.text,
            icon: item.icon,
            color: item.color,
            timeout: item.timeout,
        }

        state.value.push(nItem)

        setTimeout(() => {
            remove(nItem.key)
        }, nItem.timeout ? nItem.timeout : 30 * 1000)
    }

    return {
        state,
        remove,
        notify,
    }
}