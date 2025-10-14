export class PaginatedResponseDto<T> {
  items!: T[];
  totalCount!: number;
  pageSize!: number;
  currentPage!: number;
  totalPages!: number;
}
