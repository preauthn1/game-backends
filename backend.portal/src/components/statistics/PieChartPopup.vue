<template>
	<v-card class="dialog-v-card-border">

		<v-card-title v-if="!isHideCloseButton">
			<v-spacer></v-spacer>
			<div class="pointer" @click="close">
				<v-icon>close</v-icon>
			</div>
		</v-card-title>
		<v-card-text>
			<v-flex id="parent">
				<canvas id="canvas"></canvas>
			</v-flex>
		</v-card-text>
	</v-card>

</template>

<script lang="ts">
import Vue from 'vue';
import { Component, Prop } from 'vue-property-decorator';
import * as ChartModel from '@/models/interfaces/statistics/chart';

@Component
// https://example.invalid/jordanwillis/pen/ZeaLGj
export default class PieChartPopup extends Vue {
	dialog: boolean = true;
	@Prop(String) readonly title!: string;
	@Prop(Array) readonly datasets!: any[];
	@Prop(String) readonly xTitle!: string;
	@Prop(String) readonly yTitle!: string;
	@Prop(Object) readonly chartData!: ChartModel.ICommonChartReponse;

	Chart: any;

	isHideCloseButton: boolean = true;
	CanvasHeight: number = 125;

	initialCanvasData(myConfig: any = null) {
		if (myConfig != null) {
			if (myConfig.isHideCloseButton) {
				this.isHideCloseButton = true;
			}
			if (myConfig.CanvasHeight && myConfig.CanvasHeight > 0) {
				this.CanvasHeight = myConfig.CanvasHeight;
			}
		} else {
			this.isHideCloseButton = false;
		}

		this.destroyAndCreateNew();

		const Chart = require('chart.js');
		const canvas = <HTMLCanvasElement>document.getElementById('canvas');

		const context: any = canvas.getContext('2d');

		context.save();
		context.setTransform(1, 0, 0, 1, 0, 0);
		context.clearRect(0, 0, canvas.width, canvas.height);
		context.restore();

		canvas.height = this.CanvasHeight;
		const ctx = canvas.getContext('2d');

		const myChart = new Chart(ctx, {
			type: 'pie',
			data: {
				labels: this.chartData.labels,
				datasets: this.chartData.datasets.map((item) => {
					const generateDynamicColors = () => {
						const r = Math.floor(Math.random() * 255);
						const g = Math.floor(Math.random() * 255);
						const b = Math.floor(Math.random() * 255);
						return 'rgb(' + r + ',' + g + ',' + b + ')';
					};

					const bgColors = [
						'rgb(82, 52, 235)',
						'rgb(54, 162, 235)',
						'rgb(255, 205, 86)',
						'rgb(129, 43, 148)',
						'rgb(86, 105, 33)',
						'rgb(171, 85, 24)',
						'rgb(35, 167, 194)',
						'rgb(161, 19, 19)',
					];

					// 後面的array顏色亂數隨機產生
					item.data.forEach((s) => {
						bgColors.push(generateDynamicColors());
					});

					return {
						label: item.groupName,
						data: item.data.map((s) => {
							return s.value;
						}),

						backgroundColor: bgColors,
						hoverOffset: 4,
					};
				}),
			},
			options: {
				responsive: true,
				title: {
					display: true,
					text: this.chartData.chartName,
					fontSize: 15,
					fontFamily: '微軟正黑體, arial',
					fontColor: '#FFFFFF',
				},
				legend: {
					position: 'left',
					labels: {
						fontSize: 12,
						boxWidth: 12,
						usePointStyle: true,
						fontFamily: '微軟正黑體, arial',
						fontColor: '#FFFFFF',
					},
				},
			},
		});

		this.Chart = myChart;
	}

	destroyAndCreateNew() {
		try {
			const canvas = <HTMLCanvasElement>document.getElementById('canvas');
			canvas.remove();

			const parent = <HTMLCanvasElement>document.getElementById('parent');

			const newCanvas = document.createElement('canvas');
			newCanvas.setAttribute('id', 'canvas');
			parent.appendChild(newCanvas);
		} catch (e) {}
	}

	close() {
		this.destroyAndCreateNew();
		this.$emit('close');
	}

	mounted() {}
}
</script>
<style lang="scss" scoped>
.close {
	transition: all 300ms;
}
.close:hover {
	transform: rotate(360deg);
}
</style>
