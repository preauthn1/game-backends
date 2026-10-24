import 'package:flutter/material.dart';

import '../themes/app_colors.dart';

class Pagination extends StatelessWidget {
  final int total;
  final int perPage;
  final int currentPage;
  final ValueChanged<int> onPageChanged;

  const Pagination({
    super.key,
    required this.total,
    required this.perPage,
    required this.currentPage,
    required this.onPageChanged,
  });

  @override
  Widget build(BuildContext context) {
    final totalPages = (total / perPage).ceil();
    const maxPagesToShow = 5;

    int startPage = (currentPage - (maxPagesToShow ~/ 2)).clamp(1, totalPages);
    int endPage = (startPage + maxPagesToShow - 1).clamp(1, totalPages);

    if (endPage - startPage < maxPagesToShow - 1) {
      startPage = (endPage - maxPagesToShow + 1).clamp(1, totalPages);
    }

    return Row(
      mainAxisAlignment: MainAxisAlignment.center,
      children: [
        IconButton(
          icon: const Icon(Icons.chevron_left),
          onPressed:
              currentPage > 1 ? () => onPageChanged(currentPage - 1) : null,
        ),
        // if (startPage > 1) const Text('...'),
        ...List.generate(endPage - startPage + 1, (index) {
          final pageNumber = startPage + index;
          return GestureDetector(
            onTap: () => onPageChanged(pageNumber),
            child: Container(
              margin: const EdgeInsets.symmetric(horizontal: 8.0),
              padding: const EdgeInsets.all(8.0),
              decoration: BoxDecoration(
                shape: BoxShape.circle,
                color: currentPage == pageNumber
                    ? AppColors.primaryColor
                    : Colors.transparent,
              ),
              child: Text(
                '$pageNumber',
                style: TextStyle(
                  color:
                      currentPage == pageNumber ? Colors.black : Colors.white,
                ),
              ),
            ),
          );
        }),
        // if (endPage < totalPages) const Text('...'),
        IconButton(
          icon: const Icon(Icons.chevron_right),
          onPressed: currentPage < totalPages
              ? () => onPageChanged(currentPage + 1)
              : null,
        ),
      ],
    );
  }
}
