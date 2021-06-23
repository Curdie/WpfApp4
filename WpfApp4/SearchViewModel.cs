using Prism.Commands;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Input;

namespace WpfApp4
{
    public class SearchOptions
    {

    }

    public class SearchRepository
    {
        public async Task<List<string>> GetSearchResultsAsync(SearchOptions options, CancellationToken token)
        {
            await Task.Delay(5000, token);
            return new() { "frist", "second" };
        }
    }

    public class SearchViewModel : INotifyPropertyChanged
    {
        public SearchViewModel()
        {
            _repository = new SearchRepository();
        }
        private CancellationTokenSource cts = new();
        private SearchRepository _repository;


        private bool FirstSearchDone;

        private void OnIsSearchingChanged() => RaisePropertyChanged(SearchButtonText);
        public string SearchButtonText => IsExecuting ? "Cancel" : "Search";
        public bool IsSearching { get; private set; }
        public bool IsExecuting { get; set; }
        public SearchOptions SelectedSearchOptions { get; set; }
        public List<string> SearchResults { get; private set; } = new();
        private ICommand _searchCommand;
        public ICommand SearchCommand => _searchCommand ?? new DelegateCommand(async () => await SearchExecute());
        public async Task SearchExecute()
        {
            if (IsSearching)
            {
                cts.Cancel();
                IsExecuting = false;
                IsSearching = false;
            }
            else if (!IsExecuting)
            {
                cts = new(); // must always create a new CancellationTokenSource; old one may be "canceled"
                IsExecuting = true;
                IsSearching = true;
                var token = cts.Token;
                SearchResults.Clear();
                try
                {
                    var results = await _repository.GetSearchResultsAsync(SelectedSearchOptions, token);
                    if (!token.IsCancellationRequested)
                    {
                        foreach (var result in results)
                        {
                            SearchResults.Add(result);
                        }
                        FirstSearchDone = true;
                    }
                }
                catch (TaskCanceledException) { } // ignore 
                catch (OperationCanceledException) { } // ignore
                finally
                {
                    IsExecuting = false;
                    IsSearching = false;
                }
            }
        }

        private void RaisePropertyChanged([CallerMemberName] string propertyName = "") => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        public event PropertyChangedEventHandler PropertyChanged;
    }
}
