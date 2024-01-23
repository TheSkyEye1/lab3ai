using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace lab3ai
{
    internal class MLP
    {
        List<double> inputs = new List<double>();
        public List<Hidden> hiddens = new List<Hidden>();
        List<double> outputs = new List<double>();
        Random rng = new Random();
        double sigmoid(double x)
        {
            return 1.0/(1+Math.Exp(-x));
        }
        double d_sigmoid(double x)
        {
            return sigmoid(x) * (1 - sigmoid(x));
        }
        double LReLU(double x, double alpha = 0.01)
        {
            return x > 0 ? x : alpha * x;
        }

        double dLReLU(double x, double alpha = 0.01)
        {
            return x > 0 ? 1 : alpha;
        }


        public MLP(int inputSize, int hiddenSize, int outputSize)
        {
            rng = new Random();

            for (int i = 0; i < inputSize; i++) inputs.Add(0);
            for (int i = 0; i < outputSize; i++) outputs.Add(0);

            for (int j = 0; j < hiddenSize; j++)
            {
                Hidden h = new Hidden();

                for (int i = 0; i < inputSize; i++)
                {
                    h.inputW.Add(rng.NextDouble() - 0.3);
                }
                for (int i = 0; i < outputSize; i++)
                {
                    h.outputW.Add(rng.NextDouble() - 0.3);
                }

                h.bias = rng.NextDouble();

                hiddens.Add(h);
            }
        }

        public void setInput(double[] input)
        {
            inputs.Clear();
            inputs.AddRange(input);
        }

        public double[] getOutput() => outputs.ToArray();

        public void forwardPass()
        {
            foreach (Hidden h in hiddens)
            {
                h.value = 0;
                for (int i = 0; i < inputs.Count; i++)
                    h.value += inputs[i] * h.inputW[i];
                h.value = sigmoid(h.value + h.bias);
            }

            for (int i = 0; i < outputs.Count; i++)
            {
                outputs[i] = 0;
                foreach (Hidden h in hiddens)
                    outputs[i] += h.value * h.outputW[i];
                outputs[i] = sigmoid(outputs[i]);
            }
        }


        public void backwardPass(double[] targetValues, double LR)
        {
            double[] outputDeltas = new double[outputs.Count];
            double[] hiddenDeltas = new double[hiddens.Count];

            // Расчет outputDeltas для слоя вывода
            for (int i = 0; i < outputs.Count; i++)
            {
                double error = targetValues[i] - outputs[i];
                outputDeltas[i] = error * d_sigmoid(outputs[i]);
            }

            // Расчет hiddenDeltas для скрытого слоя
            for (int i = 0; i < hiddens.Count; i++)
            {
                double error = 0;
                for (int j = 0; j < outputs.Count; j++)
                {
                    error += outputDeltas[j] * hiddens[i].outputW[j];
                }
                hiddenDeltas[i] = d_sigmoid(hiddens[i].value) * error;
                hiddens[i].bias += LR * hiddenDeltas[i];
            }

            // Обновление весов для слоя вывода
            for (int i = 0; i < hiddens.Count; i++)
            {
                for (int j = 0; j < outputs.Count; j++)
                {
                    double change = outputDeltas[j] * hiddens[i].value;
                    hiddens[i].outputW[j] += change * LR;
                }
            }

            // Обновление весов для скрытого слоя
            for (int i = 0; i < inputs.Count; i++)
            {
                for (int j = 0; j < hiddens.Count; j++)
                {
                    double change = hiddenDeltas[j] * inputs[i];
                    hiddens[j].inputW[i] += change * LR;
                }
            }
        }


        private bool ShouldSetWeightToZero()
        {
            // Реализуйте ваш критерий для установки веса в ноль
            // Например, с вероятностью 0.2 можно устанавливать вес в ноль
            Random rand = new Random();
            return rand.NextDouble() < 0.2;
        }

        public void setHiddens(List<Hidden> list)
        {
            this.hiddens = list;
        }

        
    }
}
