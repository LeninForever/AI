using AI.Models;
using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace AI
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private const string _path = @"C:\Users\Igor\Desktop\уЧеба_4курс\1 семестр\ИИ\AI\AI\res\Output.txt";
        public MainWindow()
        {
            InitializeComponent();
        }
        private GameFieldInstanse? lastGameFieldInstanse = null;

        private void buttonStart_Click(object sender, RoutedEventArgs e)
        {

            BackgroundWorker backgroundWorker = new BackgroundWorker();

            backgroundWorker.DoWork += BackgroundWorker_DoWork;
            backgroundWorker.RunWorkerCompleted += BackgroundWorker_RunWorkerCompleted;
            buttonStart.IsEnabled = false;
            backgroundWorker.RunWorkerAsync();
        }

        private void BackgroundWorker_RunWorkerCompleted(object? sender, RunWorkerCompletedEventArgs e)
        {
            lastGameFieldInstanse = e.Result as GameFieldInstanse;

            var gameInstanseIterator = lastGameFieldInstanse;
            while (gameInstanseIterator?.Parent != null)
            {
                pathTextBox.AppendText($"{gameInstanseIterator.Current}--\r\n");
                gameInstanseIterator = gameInstanseIterator.Parent;
            }
            pathTextBox.AppendText($"{gameInstanseIterator?.Current}--\r\n");
            buttonStart.IsEnabled = true;

        }

        private void BackgroundWorker_DoWork(object? sender, DoWorkEventArgs e)
        {
             var startInstance = new GameField(4, CreateOptionsEnum.FROMFILE);

             var random = new Random();
             var range = random.Next(3, 8);

             for (int i = 0; i < 36; i++)
             {
                 startInstance.Rotate(random.Next(0, 9), random.NextDouble() >= 0.5 ? DirectionEnum.CLOCKWISE : DirectionEnum.COUNTERCLOCKWISE);
             }

             using (var writer = new StreamWriter(@"C:\Users\Igor\Desktop\уЧеба_4курс\1 семестр\ИИ\AI\AI\res\StartInstanseField.txt"))
                 writer.Write(startInstance.ToString());

            e.Result = EvristicSearch();
            //e.Result = DfsSearchAdvanced();

/*            for (int i = 1; i < 10; i++)
            {
                e.Result = DfsSearchAdvanced(i);
                if (e.Result == null)
                    continue;
                else
                    break;
            }
*/
            //e.Result = DfsSearch(3);
            //e.Result = DfsSearchAdvanced(3);

        }

        private GameFieldInstanse? EvristicSearch(GameFieldInstanse? startInstanse = null, bool isSubSearch = false)
        {
            var comparator = new GameFieldInstanseComparer();
            //var openGameFieldInstanses = new PriorityQueue<GameFieldInstanse, int>();
            var openGameFieldInstanses = new SortedList<int, List<GameFieldInstanse>>();
            int itemsCounter = 1;
            var bufferList = new List<GameFieldInstanse>();
            var closedGameFieldInstanses = new HashSet<GameFieldInstanse>();
            const int bound = 100;
            var factory = new GameFieldInstanseFactory();
            var before = GC.GetTotalMemory(true);

            if (startInstanse == null)
            {
                startInstanse = factory.CreateStartFieldInstanse(4, CreateOptionsEnum.FROMFILE);
            }
            //openGameFieldInstanses.TryGetValue(startInstanse.Path, out bufferList);
            openGameFieldInstanses.Add(startInstanse.Path, bufferList);
            bufferList.Add(startInstanse);
            //openGameFieldInstanses.Enqueue(startInstanse, startInstanse.Path);
            int maxNodesCount = 0;
            int iteration = 0;
            var watcher = Stopwatch.StartNew();

            GameFieldInstanse? lastInstanse = null;
            try
            {

                while (openGameFieldInstanses.Count != 0)
                {
                    //var currentInstanse = openGameFieldInstanses.Dequeue();
                    var minList = openGameFieldInstanses.MinBy(pair => pair.Key).Value;
                    iteration++;

                    var minArr = openGameFieldInstanses.MinBy(pair => pair.Key).Value;
                    var currentInstance = minArr.Count == 0 ? null : minArr[0];
                    if (currentInstance == null)
                    {
                        var minKey = openGameFieldInstanses.MinBy(pair => pair.Key).Key;
                        openGameFieldInstanses.Remove(minKey);
                        continue;
                    }

                    minList.RemoveAt(0);
                    itemsCounter--;

                    if (currentInstance.IsFinalInstanse())
                    {
                        lastInstanse = currentInstance;
                        break;
                    }
                    /* if (currentInstanse.IsFinalInstanse())
                     {
                         lastInstanse = currentInstanse;
                         break;
                     }*/

                    //var nextStepInstanses = factory.CreateNewFieldInstanses(currentInstanse, currentSearchDepth);
                    //              closedGameFieldInstanses.Add(currentInstanse);
                    var nextStepInstanses = factory.CreateNewFieldInstanses(currentInstance, int.MaxValue);
                    closedGameFieldInstanses.Add(currentInstance);

                    foreach (var nextStepInstanse in nextStepInstanses)
                    {
                        bool containsInClosed = closedGameFieldInstanses.Contains(nextStepInstanse);
                        closedGameFieldInstanses.TryGetValue(nextStepInstanse, out GameFieldInstanse? instanse);
                        if (instanse?.Path > nextStepInstanse.Path)
                        {
                            closedGameFieldInstanses.Remove(nextStepInstanse);
                            containsInClosed = false;
                        }
                        bool containsInOpen = false;
                        if (openGameFieldInstanses.TryGetValue(nextStepInstanse.Path, out bufferList))
                        {
                            if (bufferList.Contains(nextStepInstanse))
                                containsInOpen = true;
                        }
                        else
                        {
                            foreach (var pair in openGameFieldInstanses)
                            {
                                if (pair.Value.Contains(nextStepInstanse))
                                {
                                    if (pair.Key < nextStepInstanse.Path)
                                        containsInOpen = true;
                                    else
                                    {
                                        containsInOpen = false;
                                        pair.Value.Remove(nextStepInstanse);
                                    }
                                }
                            }
                        }

                        if (!containsInClosed && !containsInOpen)
                        {
                            //   openGameFieldInstanses.Enqueue(nextStepInstanse, nextStepInstanse.Path);
                            if (itemsCounter == bound)
                            {
                                var maxPathElementsList = openGameFieldInstanses.MaxBy(item => item.Key);
                                if (iteration == 209)
                                {
                                    var b = 2;
                                }
                                while (maxPathElementsList.Value.Count == 0)
                                {
                                    openGameFieldInstanses.Remove(openGameFieldInstanses.MaxBy(item => item.Key).Key);
                                    maxPathElementsList = openGameFieldInstanses.MaxBy(item => item.Key);

                                }
                                if (maxPathElementsList.Value.Count == 0)
                                {
                                    var a = 2;
                                }
                                var maxPathElement = maxPathElementsList.Value[0]!;
                                if (maxPathElement.Parent!.ForgottenPath == null || maxPathElement.Parent!.ForgottenPath.Path > maxPathElement.Path)
                                    maxPathElement.Parent.ForgottenPath = maxPathElement;
                                maxPathElementsList.Value.Remove(maxPathElement);
                                itemsCounter--;
                            }
                            if (!openGameFieldInstanses.TryGetValue(nextStepInstanse.Path, out bufferList))
                            {
                                openGameFieldInstanses.Add(nextStepInstanse.Path, new List<GameFieldInstanse> { nextStepInstanse });
                            }
                            else
                            {
                                bufferList.Add(nextStepInstanse);
                            }

                            itemsCounter++;
                        }

                    }

                    if (maxNodesCount < itemsCounter + closedGameFieldInstanses.Count)
                        maxNodesCount = itemsCounter + closedGameFieldInstanses.Count;
                }
            }
            catch (Exception ex)
            {

                MessageBox.Show(ex.StackTrace);
            }
            if (!isSubSearch)
            {
                foreach (var item in closedGameFieldInstanses)
                {
                    if (item.ForgottenPath != null && item.ForgottenPath.Path < lastInstanse?.Path)
                    {

                        var findedElement = EvristicSearch(new GameFieldInstanse(item.ForgottenPath.Current, null), true);
                        lastInstanse = findedElement!.Path < lastInstanse!.Path ? findedElement : lastInstanse;
                    }
                }
                watcher.Stop();
                var result = watcher.ElapsedMilliseconds;
                using (var streamWriter = new StreamWriter(_path))
                {
                    streamWriter.WriteLine($"Количество итераций {iteration}");
                    streamWriter.WriteLine($"Максимальное количество узлов в памяти {maxNodesCount}");
                    streamWriter.WriteLine($"Время выполнения {result}");
                    streamWriter.WriteLine($"Глубина{lastInstanse?.Depth}");
                }
            }
            return lastInstanse;

        }

        internal UnionGameField DfsSearchAdvanced(int currentSearchDepth = int.MaxValue)
        {
            int capacity;
            switch (currentSearchDepth)
            {
                case 1:
                    capacity = 20;
                    break;
                case 2:
                    capacity = 340;
                    break;
                case 3:
                    capacity = 5832;
                    break;
                case 4:
                    capacity = 10000;
                    break;
                case 5:
                    capacity = 235204;
                    break;
                case 6:
                    capacity = 749138;
                    break;
                case 7:
                    capacity = 5555630;
                    break;
                case 8:
                    capacity = 12927858;
                    break;
                default:
                    capacity = 100000;
                    break;
            }

            var openFieldInstanses = new Stack<UnionGameField>(capacity);
            var openFieldSet = new HashSet<UnionGameField>(capacity);
            var closedFieldInstanses = new HashSet<UnionGameField>(capacity);



            //проверяем когда достаём из стека, и когда добавляем в стек

            var factory = new GameFieldInstanseFactory();
            var startInstase = factory.CreateStartField(4, CreateOptionsEnum.FROMFILE);
            openFieldInstanses.Push(startInstase);
            openFieldSet.Add(startInstase);

            UnionGameField? lastInstanse = null;

            int maxNodesCount = 0;
            int iteration = 0;

            var watcher = Stopwatch.StartNew();
            // глубина поиска
            while (openFieldInstanses.Count != 0)
            {
                iteration++;
                var currentInstanse = openFieldInstanses.Pop();
                openFieldSet.Remove(currentInstanse);

                if (currentInstanse.IsFinalInstanse())
                {
                    lastInstanse = currentInstanse;
                    break;
                }

                //var nextStepInstanses = factory.CreateNewFieldInstanses(currentInstanse, currentSearchDepth);

                factory.CreateNewFieldInstanses(currentInstanse, currentSearchDepth, openFieldInstanses, closedFieldInstanses, openFieldSet);

                closedFieldInstanses.Add(currentInstanse);

                /*foreach (var nextStepInstanse in nextStepInstanses)
                {
                    if (!closedFieldInstanses.Contains(nextStepInstanse) && !openFieldInstanses.Contains(nextStepInstanse))
                        openFieldInstanses.Push(nextStepInstanse);
                }
                */
                if (maxNodesCount < openFieldInstanses.Count + closedFieldInstanses.Count)
                    maxNodesCount = openFieldInstanses.Count + closedFieldInstanses.Count;

            }
            watcher.Stop();
            var result = watcher.ElapsedMilliseconds;

            using (var streamWriter = new StreamWriter(_path))
            {
                streamWriter.WriteLine($"Количество итераций {iteration}");
                streamWriter.WriteLine($"Максимальное количество узлов в памяти {maxNodesCount}");
                streamWriter.WriteLine($"Время выполнения {result}");
                if (currentSearchDepth != int.MaxValue)
                    streamWriter.WriteLine("Глубина = " + currentSearchDepth);
            }

            return lastInstanse;
        }

        private GameFieldInstanse? DfsSearch(int currentSearchDepth = int.MaxValue)
        {
            Stack<GameFieldInstanse> openFieldInstanses = new Stack<GameFieldInstanse>();
            var closedFieldInstanses = new HashSet<GameFieldInstanse>();
            //проверяем когда достаём из стека, и когда добавляем в стек

            var factory = new GameFieldInstanseFactory();
            var startInstase = factory.CreateStartFieldInstanse(4, CreateOptionsEnum.FROMFILE);
            openFieldInstanses.Push(startInstase);
            GameFieldInstanse? lastInstanse = null;

            int maxNodesCount = 0;
            int iteration = 0;
            var writer = new StreamWriter(@"C:\Users\Igor\Desktop\adj.txt");
            var watcher = Stopwatch.StartNew();
            // глубина поиска
            while (openFieldInstanses.Count != 0)
            {
                iteration++;
                var currentInstanse = openFieldInstanses.Pop();
                writer.WriteLine(currentInstanse.ToString());

                if (currentInstanse.IsFinalInstanse())
                {
                    lastInstanse = currentInstanse;
                    break;
                }

                var nextStepInstanses = factory.CreateNewFieldInstanses(currentInstanse, currentSearchDepth);

                //factory.CreateNewFieldInstanses(currentInstanse, currentSearchDepth, openFieldInstanses, closedFieldInstanses);
                //  if (currentInstanse.Depth != currentSearchDepth)
                closedFieldInstanses.Add(currentInstanse);

                foreach (var nextStepInstanse in nextStepInstanses)
                {
                    if (!closedFieldInstanses.Contains(nextStepInstanse) && !openFieldInstanses.Contains(nextStepInstanse))
                        openFieldInstanses.Push(nextStepInstanse);
                }

                if (maxNodesCount < openFieldInstanses.Count + closedFieldInstanses.Count)
                    maxNodesCount = openFieldInstanses.Count + closedFieldInstanses.Count;

            }
            watcher.Stop();
            var result = watcher.ElapsedMilliseconds;

            using (var streamWriter = new StreamWriter(_path))
            {
                streamWriter.WriteLine($"Количество итераций {iteration}");
                streamWriter.WriteLine($"Максимальное количество узлов в памяти {maxNodesCount}");
                streamWriter.WriteLine($"Время выполнения {result}");
                if (currentSearchDepth != int.MaxValue)
                    streamWriter.WriteLine("Глубина = " + currentSearchDepth);
            }
            writer.Close();
            writer.Dispose();
            return lastInstanse;
        }
    }
}
