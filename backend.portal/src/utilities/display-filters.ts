import XLSX from 'xlsx';

export const commonDisplayFiltersMixin = {	
	filters: {

		integerDisplay(amount: number) {
			if (amount === 0) {
				return 0;
			}

			return amount
				? Number(amount)
						.toFixed(0)
						.toString()
						.replace(/\B(?=(\d{3})+(?!\d))/g, ",")
				: '---';
		},

		amountDisplay(myAmount: string) {

			if(myAmount === "-")
			{
				return '-';
			}

			const amount = Number(myAmount);
			
			if (amount === 0) {
				return 0;
			}
			const value = localStorage.getItem('isEnableDecimalFourPlaces');

			if(value && value != null && value === '1')
			{
				return amount
				? Number(amount)
						.toFixed(4)
						.toString()
						.replace(/(\d{1,3})(?=(\d{3})+\.)/g, '$1,')
				: // .replace(/\B(?=(\d{3})+(?!\d))/g, ',')
				  '---';
			}
			else
			{
				return amount
				? Number(amount)
						.toFixed(2)
						.toString()
						.replace(/(\d{1,3})(?=(\d{3})+\.)/g, '$1,')
				: // .replace(/\B(?=(\d{3})+(?!\d))/g, ',')
				  '---';
			}
		},

		// 已廢棄的作法
		amountDisplay4X(amount: number) {
			if (amount === 0) {
				return 0;
			}

			if(!amount) {
				return '---';
			}

			const numberText : string = Number(amount)
				.toFixed(4)
				.toString()
				.replace(/(\d{1,3})(?=(\d{3})+\.)/g, '$1,');		

			// 先固定顯示四位即可

			// try {
			// 	const last2word = numberText.slice(-2);
			// 	const last1word = numberText.slice(-1);

			// 	// 如果小數點34位都是0 則以二位顯示
			// 	if(last2word === '00') {
			// 		return Number(amount).toFixed(2).toString();
			// 	}
			// 	else
			// 	{
			// 		return numberText;
			// 	}
			//   } catch (exception) {
				
			//   } 

			return numberText;
		},

		countDisplay(count: number) {
			if (count === 0) {
				return 0;
			}
			return count
				? Number(count)
						.toString()
						.replace(/\B(?=(\d{3})+(?!\d))/g, ',')
				: // .replace(/\B(?=(\d{3})+(?!\d))/g, ',')
				  '---';
		},
	},


	methods: {		
	
					
	}


};
